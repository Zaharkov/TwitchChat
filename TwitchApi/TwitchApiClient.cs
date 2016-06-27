using RestClientHelper;
using RestSharp;
using TwitchApi.Entities;
using ResponseChecker = TwitchApi.Utils.ResponseChecker;

namespace TwitchApi
{
    public static class TwitchApiClient
    {
        private static readonly IRestClient ClientApi;
        private static readonly IRestClient ClientTmi;
        private static readonly string TwitchApiBaseUrl = Configuration.GetSetting("TwitchApiBaseUrl");
        private static readonly string TwitchTmiBaseUrl = Configuration.GetSetting("TwitchTmiBaseUrl");
        private static readonly string TwitchClientId = Configuration.GetSetting("ClientId");
        private static readonly string TwitchUrl = Configuration.GetSetting("Url");

        public static readonly string AuthorizeUrl =
            $"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id={TwitchClientId}&redirect_uri={TwitchUrl}&scope={"chat_login user_read"}";

        static TwitchApiClient()
        {
            ClientApi = new RestClientBuilder().BaseUri(TwitchApiBaseUrl).Build();
            ClientTmi = new RestClientBuilder().BaseUri(TwitchTmiBaseUrl).Build();
        }

        public static Servers GetServers()
        {
            var request = new RestRequestBuilder("/servers")
                .Method(Method.GET)
                .AddQueryParameterIfNotNull("channel", "twitch")
                .Build();

            return ExecuteTmi<Servers>(request);
        }

        public static Badges GetBadges(string channelName)
        {
            var request = new RestRequestBuilder($"/kraken/chat/{channelName}/badges")
                .Method(Method.GET)
                .Build();

            return ExecuteApi<Badges>(request);
        }

        public static User GetUserByName(string userName)
        {
            var request = new RestRequestBuilder($"/kraken/users/{userName}")
               .Method(Method.GET)
               .Build();

            return ExecuteApi<User>(request);
        }

        public static User GetUserByToken(string token)
        {
            var request = new RestRequestBuilder("/kraken/user")
               .Method(Method.GET)
               .Build();

            request.AddHeader("Authorization", "OAuth " + token);

            return ExecuteApi<User>(request);
        }

        private static T ExecuteTmi<T>(IRestRequest request) where T : class, new()
        {
            return Execute<T>(ClientTmi, request);
        }

        private static T ExecuteApi<T>(IRestRequest request) where T : class, new()
        {
            return Execute<T>(ClientApi, request);
        }

        private static T Execute<T>(IRestClient client, IRestRequest request) where T : class, new()
        {
            var result = client.Execute<T>(request);

            ResponseChecker.ValidateResponse(result);

            return result.Data;
        }

    }
}
