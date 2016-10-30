using System;
using Domain.Repositories;
using DotaClient;
using DotaClient.Friend;
using Configuration;
using Configuration.Entities;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class SteamCommand
    {
        private static readonly SteamTexts Texts = ConfigHolder.Configs.Steam.Texts;

        public static SendMessage AddSteam(MessageEventArgs e)
        {
            var message =  e.UserType.HasFlag(UserType.Broadcaster) 
                ? AddSteamBroadcaster(e.Message) 
                : AddSteamSimple(e.Username, e.Message);

            return SendMessage.GetWhisper(message);
        }

        private static string AddSteamBroadcaster(string message)
        {
            var split = message.Split(' ');

            if (split.Length != 3)
                return string.Format(Texts.IncorrectAddSyntaxAdmin, Command.ДобавитьСтим);

            var twitchName = split[1];

            if (twitchName.StartsWith(TwitchConstName.UserStartName.ToString()))
                twitchName = twitchName.TrimStart(TwitchConstName.UserStartName);

            if (!ChatterInfoRepository.Instance.IsChatterExist(twitchName))
                return string.Format(Texts.NotIntoDataBase, twitchName);

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(twitchName);

            if (sqlSteamId.HasValue)
                return string.Format(Texts.UserAlreadyAttached, twitchName, sqlSteamId.Value); 

            long msgSteamId;
            if (!long.TryParse(split[2], out msgSteamId))
                return string.Format(Texts.NotNumber, split[2]);

            var name = "";
            if (ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                return string.Format(Texts.SteamAlreadyAttached, msgSteamId);

            var result = DotaClientApi.AddFriend(Convert.ToUInt32(msgSteamId));
            return HandleAddResponse(result, msgSteamId, twitchName);
        }

        private static string AddSteamSimple(string userName, string message)
        {
            if(!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return string.Format(Texts.NotIntoDataBase, userName);

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);

            if (sqlSteamId.HasValue)
                return string.Format(Texts.UserAlreadyAttached, userName, sqlSteamId.Value);

            var split = message.Split(' ');

            if (split.Length != 2)
                return string.Format(Texts.IncorrectAddSyntaxUser, Command.ДобавитьСтим);

            long msgSteamId;
            if (!long.TryParse(split[1], out msgSteamId))
                return string.Format(Texts.NotNumber, split[1]);

            var name = "";
            if (ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                return string.Format(Texts.SteamAlreadyAttached, msgSteamId);

            var result = DotaClientApi.AddFriend(Convert.ToUInt32(msgSteamId));
            return HandleAddResponse(result, msgSteamId, userName);
        }

        private static string HandleAddResponse(FriendResponseContainer response, long steamId, string userName)
        {
            switch (response.Status)
            {
                case FriendResponseStatus.Added:
                case FriendResponseStatus.Removed:
                case FriendResponseStatus.AlreadyAdded:
                case FriendResponseStatus.CantRemove:
                    break;
                case FriendResponseStatus.Error:
                {
                    if(response.StatusCode == 41) // Ignored = 41
                        return string.Format(Texts.AddBugAfterDelete, steamId);

                    return string.Format(Texts.AddBug, steamId, response.StatusCode);
                } 
                case FriendResponseStatus.NotInFriends:
                    return string.Format(Texts.NotInFriends, steamId, response.Relationship);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response.Status), response.Status, null);
            }

            switch (response.Relationship)
            {
                case FriendResponseRelationship.None:
                case FriendResponseRelationship.Error:
                    return string.Format(Texts.AddBug, steamId, response.StatusCode);
                case FriendResponseRelationship.Banned:
                    return string.Format(Texts.InBlackList, steamId);
                case FriendResponseRelationship.Recipient:
                    return string.Format(Texts.AlreadyInRequest, steamId);
                case FriendResponseRelationship.Friend:
                    ChatterInfoRepository.Instance.AddChatterSteamId(userName, steamId);
                    return string.Format(Texts.AlreadyInFriends, steamId);
                case FriendResponseRelationship.Initiator:
                {
                    ChatterInfoRepository.Instance.AddChatterSteamId(userName, steamId);
                   
                    if (response.Status == FriendResponseStatus.AlreadyAdded)
                        return string.Format(Texts.AlreadyNeedConfirm, steamId);

                    return string.Format(Texts.NeedConfirm, steamId);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }

        public static SendMessage RemoveSteam(MessageEventArgs e)
        {
            var message =  e.UserType.HasFlag(UserType.Broadcaster) 
                ? RemoveSteamBroadcaster(e.Message) 
                : RemoveSteamSimple(e.Username, e.Message);

            return SendMessage.GetWhisper(message);
        }

        private static string RemoveSteamBroadcaster(string message)
        {
            var split = message.Split(' ');

            if (split.Length < 2 && split.Length > 3)
                return string.Format(Texts.IncorrectRemoveSyntaxAdmin, Command.УбратьСтим);

            var ignoreBug = split.Length == 3 && split[2] == "true";

            FriendResponseContainer result;
            long msgSteamId;
            if (long.TryParse(split[1], out msgSteamId))
            {
                var name = "";
                if (!ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                    return string.Format(Texts.NotAttachedSteam, msgSteamId);

                result = DotaClientApi.RemoveFriend(Convert.ToUInt32(msgSteamId), ignoreBug);
                return HandleRemoveResponse(result, msgSteamId, name);
            }

            var userName = split[1];

            if (userName.StartsWith(TwitchConstName.UserStartName.ToString()))
                userName = userName.TrimStart(TwitchConstName.UserStartName);

            if (!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return string.Format(Texts.NotIntoDataBase, userName);

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);

            if (!sqlSteamId.HasValue)
                return string.Format(Texts.NotAttachedUser, userName);

            result = DotaClientApi.RemoveFriend(Convert.ToUInt32(sqlSteamId.Value), ignoreBug);
            return HandleRemoveResponse(result, sqlSteamId.Value, userName);
        }

        private static string RemoveSteamSimple(string userName, string message)
        {
            if (!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return string.Format(Texts.NotIntoDataBase, userName);

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);
            
            if (!sqlSteamId.HasValue)
                return string.Format(Texts.NotAttachedUser, userName);

            var split = message.Split(' ');
            var ignoreBug = split.Length == 2 && split[1] == "true";

            var result = DotaClientApi.RemoveFriend(Convert.ToUInt32(sqlSteamId.Value), ignoreBug);
            return HandleRemoveResponse(result, sqlSteamId.Value, userName);
        }

        private static string HandleRemoveResponse(FriendResponseContainer response, long steamId, string userName)
        {
            switch (response.Status)
            {
                case FriendResponseStatus.Added:
                case FriendResponseStatus.Removed:
                case FriendResponseStatus.AlreadyAdded:
                    break;
                case FriendResponseStatus.CantRemove:
                    return string.Format(Texts.RemoveBugAfterDelete, steamId);
                case FriendResponseStatus.Error:
                    return string.Format(Texts.RemoveBug, steamId, response.StatusCode);
                case FriendResponseStatus.NotInFriends:
                    return string.Format(Texts.NotInFriends, steamId, response.Relationship);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response.Status), response.Status, null);
            }

            switch (response.Relationship)
            {
                case FriendResponseRelationship.None:
                    ChatterInfoRepository.Instance.DeleteChatterSteamId(userName);
                    return string.Format(Texts.Removed, steamId);
                case FriendResponseRelationship.Error:
                    return string.Format(Texts.RemoveBug, steamId, response.StatusCode);
                case FriendResponseRelationship.Banned:
                    return string.Format(Texts.InBlackList, steamId);
                case FriendResponseRelationship.Recipient:
                case FriendResponseRelationship.Friend:
                case FriendResponseRelationship.Initiator:
                    var ex = new Exception($"Twitch:{userName},steam:{steamId},relat:{response.Relationship},res:{response.StatusCode},status:{response.Status}");
                    LogRepository.Instance.LogException("DeleteSteamUserError", ex);
                    return string.Format(Texts.Wtf, steamId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }
    }
}
