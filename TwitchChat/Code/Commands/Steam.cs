using System;
using Database;
using DotaClient;
using SteamKit2;
using TwitchChat.Controls;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class SteamCommand
    {
        public static string AddSteam(MessageEventArgs e, ChatMemberViewModel userModel)
        {
            var sqlSteamId = SqLiteClient.GetChatterSteamId(e.Username);

            if (sqlSteamId.HasValue)
                return $"Пользователь twitch {e.Username} уже привязан к пользователю в steam (Id:{sqlSteamId.Value})";

            var split = e.Message.Split(' ');

            if (split.Length != 2)
                return $"Некорректный синтаксис. Введите '!{Command.AddSteam} %SteamId%'. Где %SteamId% - номер пользователя в steam. Его можно найти на профиле DotaBuff в адресе страницы";

            long msgSteamId;
            if (!long.TryParse(split[1], out msgSteamId))
                return $"Номер пользователя в steam должен быть обычным числом. Передано '{split[1]}'";

            if (SqLiteClient.IsSteamIdAttachedToChatter(msgSteamId))
                return $"Номер пользователя в steam '{msgSteamId}' уже привязан к одному из пользователей в twitch";

            var result = DotaClientApi.AddFriend(Convert.ToUInt32(msgSteamId));

            switch (result)
            {
                case EFriendRelationship.None:
                case EFriendRelationship.Max:
                case EFriendRelationship.SuggestedFriend:
                    return $"Во время добавления '{msgSteamId}' в steam произошла ошибка. Попробуйте позже";
                case EFriendRelationship.Blocked:
                case EFriendRelationship.Ignored:
                case EFriendRelationship.IgnoredFriend:
                    return $"Пользователь steam '{msgSteamId}' находится в черном списке или заблокирован. Очень жаль";
                case EFriendRelationship.RequestRecipient:
                    return $"Уже был запрос в друзья со стороны пользователя steam '{msgSteamId}', но подтверждение заявки не сработало. Попробуйте позже";
                case EFriendRelationship.Friend:
                    SqLiteClient.AddChatterSteamId(e.Username, msgSteamId);
                    return $"Пользователь steam '{msgSteamId}' уже находится в списке друзей. Теперь он привязан к Вам";
                case EFriendRelationship.RequestInitiator:
                    SqLiteClient.AddChatterSteamId(e.Username, msgSteamId);
                    return $"Запрос на добавление в список друзей отослан. Теперь пользователь steam '{msgSteamId}' привязан к Вам. Зайдите в steam и подтвердите заявку";
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
