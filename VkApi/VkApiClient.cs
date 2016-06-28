
using System.Collections.Generic;
using RestClientHelper;
using RestSharp;
using VkApi.Entities;

namespace VkApi
{
    public class VkApiClient
    {
        private static readonly string VkClientId = Configuration.GetSetting("VkClientId");
        private static readonly string VkClientSecret = Configuration.GetSetting("VkClientSecret");
        private static readonly string Url = Configuration.GetSetting("Url");

        private readonly IRestClient _client;
        private readonly string _accessToken;

        public static string AuthorizeUrl =
            $"https://oauth.vk.com/authorize?client_id={VkClientId}&display=page&redirect_uri={Url}&scope=audio&response_type=code";
        public static string GetTokenUrl =
            $"https://oauth.vk.com/access_token?client_id={VkClientId}&client_secret={VkClientSecret}&redirect_uri={Url}&code=";

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
                .AddQueryParameterIfNotNull("access_token", _accessToken)
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
