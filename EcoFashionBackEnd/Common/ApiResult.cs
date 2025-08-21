namespace EcoFashionBackEnd.Common
{
    public record ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string>? Errors { get; set; } // Bổ sung trường hợp nhiều lỗi


        // Trường hợp thành công, có dữ liệu
        public static ApiResult<T> Succeed(T? result)
        {
            return new ApiResult<T> { Success = true, Result = result };
        }
        // Trường hợp thành công, không có dữ liệu
        public static ApiResult<T> Error(T? result)
        {
            return new ApiResult<T> { Success = false, Result = result };
        }


        // Trường hợp thất bại với một thông báo lỗi duy nhất
        public static ApiResult<T> Fail(string errorMessage)
        {
            return new ApiResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        public static ApiResult<object> Fail(Exception ex)
        {
            return new ApiResult<object>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }


        // Trường hợp thất bại với danh sách lỗi (ví dụ: lỗi validation)
        public static ApiResult<T> Fail(List<string> errors)
        {
            return new ApiResult<T>
            {
                Success = false,
                Errors = errors
            };
        }

    }
}