using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Domain.Repositories;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class SqlCommand
    {
        public static SendMessage Sql(MessageEventArgs e)
        {
            var sql = e.Message.Replace($"{TwitchConstName.Command}{Command.Sql}", "").Trim();

            var result = sql.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)
                ? CustomSqlRepository.Instance.Get(sql)
                : CustomSqlRepository.Instance.Change(sql);

            return SendMessage.GetWhisper(result);
        }
    }
}
