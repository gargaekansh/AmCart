namespace AmCart.Identity.API.Models
{
    /// <summary>
    /// Standardized API response format.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        private ApiResponse(bool success, string message, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// Creates a success response.
        /// </summary>
        public static ApiResponse SuccessResponse(object data, string message = "Request successful")
        {
            return new ApiResponse(true, message, data);
        }

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        public static ApiResponse FailureResponse(string message, string details = null)
        {
            return new ApiResponse(false, message, details != null ? new { details } : null);
        }
    }

}
