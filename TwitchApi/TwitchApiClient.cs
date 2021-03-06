﻿using CommonHelper;
using Configuration;
using RestClientHelper;
using RestSharp;
using TwitchApi.Entities;

namespace TwitchApi
{
    public static class TwitchApiClient
    {
        private static readonly IRestClient ClientApi;
        private static readonly IRestClient ClientTmi;
        private static readonly string TwitchApiBaseUrl = ConfigHolder.Configs.Global.Params.TwitchApiBaseUrl;
        private static readonly string TwitchTmiBaseUrl = ConfigHolder.Configs.Global.Params.TwitchTmiBaseUrl;
        private static readonly string TwitchClientId = ConfigHolder.Configs.Global.Params.ClientId;
        private static readonly string TwitchUrl = ConfigHolder.Configs.Global.Params.Url;

        private static string _accessToken;

        public static readonly string AuthorizeUrl =
            $"https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id={TwitchClientId}&redirect_uri={TwitchUrl}&scope=chat_login user_read channel_read channel_subscriptions user_follows_edit";

        static TwitchApiClient()
        {
            ClientApi = new RestClientBuilder().BaseUri(TwitchApiBaseUrl).Build();
            ClientApi.AddDefaultHeader("Client-ID", TwitchClientId);

            ClientTmi = new RestClientBuilder().BaseUri(TwitchTmiBaseUrl).Build();
        }

        public static void SetToken(string token)
        {
            Check.ForEmptyString(token);

            _accessToken = token;
        }

        public static string GetToken()
        {
            Check.ForEmptyString(_accessToken);

            return _accessToken;
        }

        public static Servers GetServers()
        {
            var request = new RestRequestBuilder("/servers")
                .Method(Method.GET)
                .AddQueryParameterIfNotNull("channel", "twitch")
                .Build();

            return ExecuteTmi<Servers>(request);
        }

        public static ChatterInfo GetUsersList(string channel)
        {
            Check.ForEmptyString(channel);

            var request = new RestRequestBuilder($"/group/user/{channel}/chatters")
                .Method(Method.GET)
                .Build();

            return ExecuteTmi<ChatterInfo>(request);
        }

        public static Badges GetBadges(string channelName)
        {
            Check.ForEmptyString(channelName);

            var request = new RestRequestBuilder($"/kraken/chat/{channelName}/badges")
                .Method(Method.GET)
                .Build();

            return ExecuteApi<Badges>(request);
        }

        public static User GetUserByName(string userName)
        {
            Check.ForEmptyString(userName, nameof(userName));

            var request = new RestRequestBuilder($"/kraken/users/{userName}")
               .Method(Method.GET)
               .Build();

            return ExecuteApi<User>(request);
        }

        public static User GetUserByToken()
        {
            Check.ForEmptyString(_accessToken, nameof(_accessToken));

            var request = new RestRequestBuilder("/kraken/user")
               .Method(Method.GET)
               .Build();

            request.AddHeader("Authorization", "OAuth " + _accessToken);

            return ExecuteApi<User>(request);
        }

        public static StreamInfo GetStreamInfo(string name)
        {
            Check.ForEmptyString(name, nameof(name));

            var request = new RestRequestBuilder($"/kraken/streams/{name}")
               .Method(Method.GET)
               .Build();

            return ExecuteApi<StreamInfo>(request);
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

            Utils.ResponseChecker.ValidateResponse(result);

            return result.Data;
        }

    }
}
