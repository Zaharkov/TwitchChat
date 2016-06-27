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
            RestClientHelper.ResponseChecker.ValidateResponse(response);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
                throw new ErrorResponseDataException(error);
            }
        }
    }
}
