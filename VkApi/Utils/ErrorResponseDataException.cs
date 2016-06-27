using System;
using RestSharp.Serializers;
using VkApi.Entities;

namespace VkApi.Utils
{
    public class ErrorResponseDataException : Exception
    {
        public ErrorResponseDataException(Error data)
            : base($"There is error in response. Message: {data.Message}, params: {new JsonSerializer().Serialize(data.RequestParams)}, code: {data.Code}")
        {
        }
    }
}
