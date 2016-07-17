using SteamKit2;

namespace DotaClient.Friend
{
    public class FriendResponseContainer
    {
        public EFriendRelationship Relationship { get; }
        public EResult Result { get; }
        public FriendResponseStatus Status { get; }

        public FriendResponseContainer(EResult result, EFriendRelationship relationship, FriendResponseStatus status)
        {
            Result = result;
            Relationship = relationship;
            Status = status;
        }
    }
}
