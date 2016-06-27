using System;
using RestSharp;

namespace RestClientHelper
{
    public class NullResponseDataException : Exception
    {
        public NullResponseDataException(IRestResponse response, Type dataType)
            : base($"Response data was null. May be it couldn't be deserialized to {dataType}. Status code: {response.StatusCode}, Content: {response.Content}")
        {
        }
    }
}
