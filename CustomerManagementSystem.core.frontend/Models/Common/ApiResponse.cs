namespace CustomerManagementSystem.core.frontend.Models.Common
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class ApiMessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
