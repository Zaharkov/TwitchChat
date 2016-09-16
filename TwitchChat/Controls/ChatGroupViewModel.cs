using System;
using System.Linq;

namespace TwitchChat.Controls
{
    using System.Collections.ObjectModel;

    //  This can be expanded to include icons for chat groups. maybe if chatters are separated by badges
    public class ChatGroupViewModel
    {
        private readonly object _lock = new object();

        public string Name { get; set; }
        public ObservableCollection<ChatMemberViewModel> Members { get; }

        public ChatGroupViewModel()
        {
            Members = new ObservableCollection<ChatMemberViewModel>();
        }

        public ObservableCollection<ChatMemberViewModel> Get()
        {
            lock (_lock)
            {
                return Members;
            }
        }

        public bool Any(Func<ChatMemberViewModel, bool> predicate)
        {
            lock (_lock)
            {
                return Members.Any(predicate);
            }
        }

        public ChatMemberViewModel FirstOrDefault(Func<ChatMemberViewModel, bool> predicate)
        {
            lock (_lock)
            {
                return Members.FirstOrDefault(predicate);
            }
        }

        public void Add(ChatMemberViewModel model)
        {
            lock (_lock)
            {
                Members.Add(model);
            }
        }

        public void Remove(ChatMemberViewModel model)
        {
            lock (_lock)
            {
                Members.Remove(model);
            }
        }
    }
}
