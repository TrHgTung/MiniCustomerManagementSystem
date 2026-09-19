using System.Text.Json;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomerManagementSystem.core.backend.Helpers.Attributes
{
    /// <summary>
    /// Caches an action response by the required X-Idempotency-Key header.
    /// </summary>
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private const string IdempotencyHeaderName = "X-Idempotency-Key";
        private readonly IIdempotencyService _idempotencyService;
        private readonly ILogger<IdempotencyFilter> _logger;

        public int ExpireMinutes { get; set; } = 60;

        public IdempotencyFilter(IIdempotencyService idempotencyService, ILogger<IdempotencyFilter> logger)
        {
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idempotencyKey = ExtractIdempotencyKey(context.HttpContext.Request);
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Header 'X-Idempotency-Key' is required for this request."
                });
                return;
            }

            var requestPath = context.HttpContext.Request.Path.ToString();
            var existingRecord = await _idempotencyService.GetAsync(idempotencyKey);

            if (existingRecord != null)
            {
                if (existingRecord.StatusCode == 0)
                {
                    _logger.LogWarning("Idempotency key '{Key}' is being processed by another request.", idempotencyKey);
                    context.Result = new ConflictObjectResult(new
                    {
                        message = "A request with this X-Idempotency-Key is being processed."
                    });
                    return;
                }

                _logger.LogInformation("Idempotency replay: key '{Key}', StatusCode={StatusCode}", idempotencyKey, existingRecord.StatusCode);
                context.HttpContext.Response.Headers["X-Idempotency-Replayed"] = "true";
                context.Result = new ContentResult
                {
                    StatusCode = existingRecord.StatusCode,
                    Content = existingRecord.ResponseBody,
                    ContentType = "application/json"
                };
                return;
            }

            var created = await _idempotencyService.TryCreatePendingAsync(
                idempotencyKey,
                requestPath,
                TimeSpan.FromMinutes(ExpireMinutes));

            if (!created)
            {
                context.Result = new ConflictObjectResult(new
                {
                    message = "A request with this X-Idempotency-Key is being processed."
                });
                return;
            }

            var executedContext = await next();
            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                _logger.LogWarning("Action threw an exception for idempotency key '{Key}'. Removing pending record.", idempotencyKey);
                await _idempotencyService.RemoveAsync(idempotencyKey);
                return;
            }

            if (executedContext.Result is ObjectResult objectResult)
            {
                var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
                var responseBody = JsonSerializer.Serialize(objectResult.Value, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _idempotencyService.CompleteAsync(idempotencyKey, statusCode, responseBody);
                _logger.LogInformation("Idempotency key '{Key}' stored with StatusCode={StatusCode}.", idempotencyKey, statusCode);
            }
            else if (executedContext.Result is StatusCodeResult statusCodeResult)
            {
                await _idempotencyService.CompleteAsync(idempotencyKey, statusCodeResult.StatusCode, string.Empty);
            }
            else
            {
                _logger.LogWarning("Idempotency key '{Key}': unsupported result type ({ResultType}). Removing pending record.", idempotencyKey, executedContext.Result?.GetType().Name ?? "null");
                await _idempotencyService.RemoveAsync(idempotencyKey);
            }
        }

        private static string? ExtractIdempotencyKey(HttpRequest request)
        {
            if (request.Headers.TryGetValue(IdempotencyHeaderName, out var key) && !string.IsNullOrWhiteSpace(key))
            {
                return key.ToString().Trim();
            }

            return null;
        }
    }
}
