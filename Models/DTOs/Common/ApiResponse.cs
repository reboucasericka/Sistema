namespace SistemaAPI.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public bool IsSuccess => Success;
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; } = 200;
        
        public static ApiResponse<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }
        
        public static ApiResponse<T> ErrorResult(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
        
        public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
        {
            return ErrorResult(message, statusCode, errors);
        }
    }
}
