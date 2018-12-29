namespace CS_API_Server.TransportModels
{
    public class StandardResponse
    {
        public bool Error { get; set; }
        public string ErrorDetail { get; set; }
        public string Message { get; set; }

        public StandardResponse(string message)
        {
            Message = message;
        }
        public StandardResponse(bool error, string errorDetail)
        {
            Error = error;
            ErrorDetail = errorDetail;
        }
    }
}
