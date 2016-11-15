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

            var messages = command.Type == CommandType.Write && e != null
                ? command.Texts.Any(t => e.UserType.HasFlag(t.Access))
                    ? command.Texts.Where(t => e.UserType.HasFlag(t.Access)).ToList()
                    : command.Texts.Where(t => t.Access.HasFlag(UserType.Default)).ToList()
                : command.Texts;

            if (messages.Any())
            {
                var randomMes = new Random();
                var message = messages[randomMes.Next(0, messages.Count)];

                return SendMessage.GetMessage(string.Format(message.Text, args.Cast<object>().ToArray()));
            }

            return SendMessage.None;
        }
    }
}
