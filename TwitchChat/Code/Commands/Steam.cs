using System;
using Domain.Repositories;
using DotaClient;
using DotaClient.Friend;
using Twitchiedll.IRC;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class SteamCommand
    {
        public static SendMessage AddSteam(MessageEventArgs e)
        {
            var message =  e.UserType.HasFlag(UserType.Broadcaster) ? AddSteamBroadcaster(e.Message) : AddSteamSimple(e.Username, e.Message);
            return SendMessage.GetWhisper(message);
        }

        private static string AddSteamBroadcaster(string message)
        {
            var split = message.Split(' ');

            if (split.Length != 3)
                return $"Некорректный синтаксис. Введите '!{Command.ДобавитьСтим} %TwitchName% %SteamId%'. Где %TwitchName% имя пользователя twitch, а %SteamId% - номер пользователя в steam. Его можно найти на профиле DotaBuff в адресе страницы";

            var twitchName = split[1];

            if (!ChatterInfoRepository.Instance.IsChatterExist(twitchName))
                return $"Пользователя twitch {twitchName} пока не в базе данных. Подождите сутки для обновления данных";

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(twitchName);

            if (sqlSteamId.HasValue)
                return $"Пользователь twitch {twitchName} уже привязан к пользователю в steam (Id:{sqlSteamId.Value})";

            long msgSteamId;
            if (!long.TryParse(split[2], out msgSteamId))
                return $"Номер пользователя в steam должен быть обычным числом. Передано '{split[2]}'";

            var name = "";
            if (ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                return $"Номер пользователя в steam '{msgSteamId}' уже привязан к одному из пользователей в twitch";

            var result = DotaClientApi.AddFriend(Convert.ToUInt32(msgSteamId));
            return HandleAddResponse(result, msgSteamId, twitchName);
        }

        private static string AddSteamSimple(string userName, string message)
        {
            if(!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return $"Пользователя twitch {userName} пока не в базе данных. Подождите сутки для обновления данных";

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);

            if (sqlSteamId.HasValue)
                return $"Пользователь twitch {message} уже привязан к пользователю в steam (Id:{sqlSteamId.Value})";

            var split = message.Split(' ');

            if (split.Length != 2)
                return $"Некорректный синтаксис. Введите '!{Command.ДобавитьСтим} %SteamId%'. Где %SteamId% - номер пользователя в steam. Его можно найти на профиле DotaBuff в адресе страницы";

            long msgSteamId;
            if (!long.TryParse(split[1], out msgSteamId))
                return $"Номер пользователя в steam должен быть обычным числом. Передано '{split[1]}'";

            var name = "";
            if (ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                return $"Номер пользователя в steam '{msgSteamId}' уже привязан к одному из пользователей в twitch";

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
                        return $"Похоже пользователь steam {steamId} уже был ранее удален из друзей без подтверждения заявки. Это вызывает баг в steam, который можно исправить, если заблокировать и потом разблокировать все связи с ним в steam";

                    return $"Во время добавления '{steamId}' произошла ошибка с кодом: {response.StatusCode}. Попробуйте позже";
                }
                case FriendResponseStatus.NotInFriends:
                    return $"Пользователь steam '{steamId}' не находится в списке друзей. Статус: {response.Relationship}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(response.Status), response.Status, null);
            }

            switch (response.Relationship)
            {
                case FriendResponseRelationship.None:
                case FriendResponseRelationship.Error:
                    return $"Во время добавления '{steamId}' в steam произошла ошибка с кодом: {response.StatusCode}. Попробуйте позже";
                case FriendResponseRelationship.Banned:
                    return $"Пользователь steam '{steamId}' находится в черном списке или заблокирован. Очень жаль";
                case FriendResponseRelationship.Recipient:
                    return $"Уже был запрос в друзья со стороны пользователя steam '{steamId}', но подтверждение заявки не сработало. Попробуйте позже";
                case FriendResponseRelationship.Friend:
                    ChatterInfoRepository.Instance.AddChatterSteamId(userName, steamId);
                    return $"Пользователь steam '{steamId}' уже находится в списке друзей. Теперь он привязан к Вам";
                case FriendResponseRelationship.Initiator:
                {
                    ChatterInfoRepository.Instance.AddChatterSteamId(userName, steamId);
                   
                    if (response.Status == FriendResponseStatus.AlreadyAdded)
                        return $"Запрос на добавление в список друзей уже был отослан ранее. Теперь пользователь steam '{steamId}' привязан к Вам. Зайдите в steam и подтвердите заявку";

                    return $"Запрос на добавление в список друзей отослан. Теперь пользователь steam '{steamId}' привязан к Вам. Зайдите в steam и подтвердите заявку";
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }

        public static SendMessage RemoveSteam(MessageEventArgs e)
        {
            var message =  e.UserType.HasFlag(UserType.Broadcaster) ? RemoveSteamBroadcaster(e.Message) : RemoveSteamSimple(e.Username, e.Message);
            return SendMessage.GetWhisper(message);
        }

        private static string RemoveSteamBroadcaster(string message)
        {
            var split = message.Split(' ');

            if (split.Length < 2 && split.Length > 3)
                return $"Некорректный синтаксис. Введите '!{Command.УбратьСтим} %User%'. Где %User% - или имя пользователя twitch или номер пользователя в steam (его можно найти на профиле DotaBuff в адресе страницы)";

            var ignoreBug = split.Length == 3 && split[2] == "true";

            FriendResponseContainer result;
            long msgSteamId;
            if (long.TryParse(split[1], out msgSteamId))
            {
                var name = "";
                if (!ChatterInfoRepository.Instance.IsSteamIdAttachedToChatter(msgSteamId, ref name))
                    return $"Номер пользователя в steam '{msgSteamId}' не привязан ни к одному из пользователей в twitch";

                result = DotaClientApi.RemoveFriend(Convert.ToUInt32(msgSteamId), ignoreBug);
                return HandleRemoveResponse(result, msgSteamId, name);
            }

            var userName = split[1];

            if (!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return $"Пользователя twitch {userName} пока не в базе данных. Подождите сутки для обновления данных";

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);

            if (!sqlSteamId.HasValue)
                return $"Пользователь twitch {userName} не привязан к пользователю в steam";

            result = DotaClientApi.RemoveFriend(Convert.ToUInt32(sqlSteamId.Value), ignoreBug);
            return HandleRemoveResponse(result, sqlSteamId.Value, userName);
        }

        private static string RemoveSteamSimple(string userName, string message)
        {
            if (!ChatterInfoRepository.Instance.IsChatterExist(userName))
                return $"Пользователя twitch {userName} пока не в базе данных. Подождите сутки для обновления данных";

            var sqlSteamId = ChatterInfoRepository.Instance.GetChatterSteamId(userName);

            if (!sqlSteamId.HasValue)
                return $"Пользователь twitch {userName} не привязан к пользователю в steam";

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
                    return $"Удаление '{steamId}' из друзей повлечёт за собой баг, так как пользователь ещё не подтвердил заявку в друзья. Нельзя будет добавить его снова пока не заблокировать и потом разблокировать все связи с ним в steam. Для игнорирования этого бага введите в конце команды параметр 'true'";
                case FriendResponseStatus.Error:
                    return $"Во время удаления '{steamId}' из друзей произошла ошибка с кодом: {response.StatusCode}. Попробуйте позже";
                case FriendResponseStatus.NotInFriends:
                    return $"Пользователь steam '{steamId}' не находится в списке друзей. Статус: {response.Relationship}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(response.Status), response.Status, null);
            }

            switch (response.Relationship)
            {
                case FriendResponseRelationship.None:
                    ChatterInfoRepository.Instance.DeleteChatterSteamId(userName);
                    return $"Пользователь steam '{steamId}' успешно удален из друзей в steam и откреплён от пользователя twitch {userName}";
                case FriendResponseRelationship.Error:
                    return $"Во время удаления '{steamId}' из друзей в steam произошла ошибка с кодом: {response.StatusCode}. Попробуйте позже";
                case FriendResponseRelationship.Banned:
                    return $"Пользователь steam '{steamId}' находится в черном списке или заблокирован. Очень жаль";
                case FriendResponseRelationship.Recipient:
                case FriendResponseRelationship.Friend:
                case FriendResponseRelationship.Initiator:
                    LogRepository.Instance.LogException("DeleteSteamUserError", new Exception($"Twitch:{userName},steam:{steamId},relat:{response.Relationship},res:{response.StatusCode},status:{response.Status}"));
                    return $"Удаление {steamId} вернуло неожиданный ответ...как Вы это сделали? Запишу ка я эту ошибку в базу...Сообщите о ней бродкастеру!";
                default:
                    throw new ArgumentOutOfRangeException(nameof(response), response, null);
            }
        }
    }
}
