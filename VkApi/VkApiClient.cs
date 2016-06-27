
using System.Collections.Generic;
using RestClientHelper;
using RestSharp;
using VkApi.Entities;

namespace VkApi
{
    public class VkApiClient
    {
        private readonly IRestClient _client;
        private readonly string _accessToken;

        public VkApiClient(string accessToken)
        {
            Check.ForNullReference(accessToken);

            _client = new RestClientBuilder()
                .BaseUri("https://api.vk.com/method")
                .Build();

            _accessToken = accessToken;
        }

        public List<User> GetBroadcastList()
        {
            var request = new RestRequestBuilder("audio.getBroadcastList")
                .Method(Method.GET)
                .AddQueryParameterIfNotNull("access_token", "6d54a52fb9186a55ea914a191971fecfced3ce86fd86b86224cb5a92b85da60d4af6201f030b5c8917b2c")
                .AddQueryParameterIfNotNull("active", "1")
                .AddQueryParameterIfNotNull("filter", "all")
                .Build();

            return Execute<List<User>>(request);
        }

        private T Execute<T>(IRestRequest request) where T : class, new()
        {
            var result = _client.Execute<Response<T>>(request);

            Utils.ResponseChecker.ValidateResponse(result);

            return result.Data.Data;
        }
    }
}
