
using System.Collections.Generic;
using RestClientHelper;
using RestSharp;
using VkApi.Entities;

namespace VkApi
{
    public static class VkApiClient
    {
        private static readonly string VkClientId = Configuration.GetSetting("VkClientId");
        private static readonly string VkClientSecret = Configuration.GetSetting("VkClientSecret");
        private static readonly string Url = Configuration.GetSetting("Url");

        private static readonly IRestClient ApiClient;
        private static readonly IRestClient AuthClient;
        private static string _accessToken;

        public static string AuthorizeUrl =
            $"https://oauth.vk.com/authorize?client_id={VkClientId}&display=page&redirect_uri={Url}&scope=audio&response_type=code";

        static VkApiClient()
        {
            ApiClient = new RestClientBuilder()
                .BaseUri("https://api.vk.com/method")
                .Build();

            AuthClient = new RestClientBuilder()
                .BaseUri("https://oauth.vk.com")
                .Build();
        }

        public static Token GetToken(string code)
        {
            var request = new RestRequestBuilder("access_token")
                .Method(Method.GET)
                .AddQueryParameterIfNotNull("client_id", VkClientId)
                .AddQueryParameterIfNotNull("client_secret", VkClientSecret)
                .AddQueryParameterIfNotNull("redirect_uri", Url)
                .AddQueryParameterIfNotNull("code", code)
                .Build();

            var result = ExecuteAuth<Token>(request);

            _accessToken = result.AccessToken;

            return result;
        }

        public static List<User> GetBroadcastList()
        {
            Check.ForNullReference(_accessToken, nameof(_accessToken));

            var request = new RestRequestBuilder("audio.getBroadcastList")
                .Method(Method.GET)
                .AddQueryParameterIfNotNull("access_token", _accessToken)
                .AddQueryParameterIfNotNull("active", "1")
                .AddQueryParameterIfNotNull("filter", "all")
                .Build();

            return ExecuteApi<List<User>>(request);
        }

        private static T ExecuteApi<T>(IRestRequest request) where T : class, new()
        {
            var result = ApiClient.Execute<Response<T>>(request);

            Utils.ResponseChecker.ValidateResponse(result);

            return result.Data.Data;
        }

        private static T ExecuteAuth<T>(IRestRequest request) where T : class, new()
        {
            var result = AuthClient.Execute<T>(request);

            Utils.ResponseChecker.ValidateResponse(result);

            return result.Data;
        }
    }
}
