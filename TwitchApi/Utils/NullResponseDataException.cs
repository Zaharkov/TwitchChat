using System;
using System.Net;
using TwitchApi.Entities;

namespace TwitchApi.Utils
{
    public class ErrorResponseDataException : Exception
    {
        public string ErrorMessage;
        public string Error;
        public HttpStatusCode Status;

        public ErrorResponseDataException(ErrorResponse data)
            : base($"There is error in response. Message: {data.Message}, error: {data.Error}, status: {(int)data.Status}")
        {
            ErrorMessage = data.Message;
            Error = data.Error;
            Status = data.Status;
        }
    }
}
