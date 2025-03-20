namespace HotelReservation.Helpers
{
    public class ResponseResult
    {
        public bool Success { get; set; }
        public bool IsWarning { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ResponseResult Ok(string message) => new ResponseResult { Success = true, Message = message };
        public static ResponseResult Fail(string message) => new ResponseResult { Success = false, Message = message };
        public static ResponseResult Warn(string message) => new ResponseResult { Success = true, IsWarning = true, Message = message };
    }
}
