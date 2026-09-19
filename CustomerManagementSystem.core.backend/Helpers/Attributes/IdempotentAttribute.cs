using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomerManagementSystem.core.backend.Helpers.Attributes
{
    /// <summary>
    /// Marks an action as idempotent. Requests must include X-Idempotency-Key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IdempotentAttribute : Attribute, IFilterFactory
    {
        /// <summary>
        /// Lifetime of the idempotency key in minutes.
        /// </summary>
        public int ExpireMinutes { get; set; } = 60;

        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<IdempotencyFilter>();
            filter.ExpireMinutes = ExpireMinutes;
            return filter;
        }
    }
}
