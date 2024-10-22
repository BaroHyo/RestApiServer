namespace RestApiServer.Utilities
{
    public class ErrorResponse
    {
        public bool Error { get; set; }
        public string Message { get; set; }

        public ErrorResponse(bool error, string message)
        {
            Error = error;
            Message = message;
        }
    }

}
