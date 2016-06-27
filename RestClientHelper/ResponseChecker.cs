using System.Net;
using RestSharp;

namespace RestClientHelper
{
    public static class ResponseChecker
    {
        public static void ValidateResponse<T>(IRestResponse<T> response)
        {
            if (response.ResponseStatus == ResponseStatus.Error)
                throw new WebException(response.ErrorMessage, response.ErrorException);

            if (response.Data == null)
                throw new NullResponseDataException(response, typeof(T));
        }
    }
}
