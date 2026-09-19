namespace CustomerManagementSystem.core.frontend.Services.Http
{
    /// <summary>
    /// Adds a key to requests that can change server state. A caller can set the
    /// header beforehand to reuse the same key when it deliberately retries.
    /// </summary>
    public class IdempotencyKeyHandler : DelegatingHandler
    {
        public const string HeaderName = "X-Idempotency-Key";

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (IsUnsafeMethod(request.Method) && !request.Headers.Contains(HeaderName))
            {
                request.Headers.TryAddWithoutValidation(HeaderName, Guid.NewGuid().ToString());
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static bool IsUnsafeMethod(HttpMethod method) =>
            method == HttpMethod.Post ||
            method == HttpMethod.Put ||
            method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
            method == HttpMethod.Delete;
    }
}
