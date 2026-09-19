using CustomerManagementSystem.core.backend.Helpers.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CustomerManagementSystem.core.backend.Configurations
{
    /// <summary>
    /// Xử lý chỉ việc thêm header X-Idempotency-Key vào swagger ui (
    /// (cho các endpoint có [Idempotent])
    /// </summary>
    public class IdempotencyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo
                .GetCustomAttributes(typeof(IdempotentAttribute), false)
                .OfType<IdempotentAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = $"Idempontency key để ngăn chặn trùng lặp request. Hết hạn sau {attribute.ExpireMinutes} phút",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                }
            });
        }
    }
}
