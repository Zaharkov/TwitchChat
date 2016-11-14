using System;
using System.Collections.Generic;
using System.Linq;
using Configuration.Entities;
using TwitchChat.Controls;
using Twitchiedll.IRC.Enums;
using Twitchiedll.IRC.Events;

namespace TwitchChat.Code.Commands
{
    public static class CustomCommandHandler
    {
        public static SendMessage CreateCommand(CustomCommand<CommandType, UserType, DelayType> command, MessageEventArgs e, ChannelViewModel model)
        {
            var args = new List<string>();

            foreach (var param in command.CommandParams)
            {
                switch (param.Type)
                {
                    case ParamType.RandomNumber:
                    {
                        var random = new Random();
                        args.Add(random.Next(param.Min, param.Max+1).ToString());
                        break;
                    }
                    case ParamType.RandomUser:
                    {
                        var random = new Random();
                        var chatters = model.GetAllChatters();
                        args.Add(chatters.Count == 0 ? string.Empty : chatters[random.Next(0, chatters.Count)].Name);
                        break;
                    }
                    case ParamType.UserName:
                    {
                        args.Add(e == null ? string.Empty : e.Username);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return SendMessage.GetMessage(string.Format(command.Text, args.Cast<object>().ToArray()));
        }
    }
}
