using System.Net;
using RestClientHelper;
using RestSharp;
using VkApi.Entities;

namespace VkApi.Utils
{
    internal static class ResponseChecker
    {
        public static void ValidateResponse<T>(IRestResponse<Response<T>> response)
        {
            if (response.ResponseStatus == ResponseStatus.Error)
                throw new WebException(response.ErrorMessage, response.ErrorException);

            if (response.Data.Error != null)
                throw new ErrorResponseDataException(response.Data.Error);

            if (response.Data.Data == null)
                throw new NullResponseDataException(response, typeof(T)); 
        }

        public static void ValidateResponse<T>(IRestResponse<T> response)
        {
            if (response.ResponseStatus == ResponseStatus.Error)
                throw new WebException(response.ErrorMessage, response.ErrorException);

            if (response.Data == null)
                throw new NullResponseDataException(response, typeof(T));
        }

        public static void ValidateResponse(IRestResponse response)
        {
            if (response.ResponseStatus == ResponseStatus.Error)
                throw new WebException(response.ErrorMessage, response.ErrorException);

            if (response.Content == null)
                throw new NullResponseDataException(response, typeof(string));
        }
    }
}
