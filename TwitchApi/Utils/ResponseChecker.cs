using System.Net;
using Newtonsoft.Json;
using RestSharp;
using TwitchApi.Entities;

namespace TwitchApi.Utils
{
    public static class ResponseChecker
    {
        public static void ValidateResponse<T>(IRestResponse<T> response)
        {
            if (response.ResponseStatus == ResponseStatus.Error)
                throw new WebException(response.ErrorMessage, response.ErrorException);

            if (response.Data == null)
                throw new NullResponseDataException(response, typeof(T));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
                throw new ErrorResponseDataException(error);
            }
        }
    }
}
