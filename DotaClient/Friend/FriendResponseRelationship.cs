using System;
using SteamKit2;

namespace DotaClient.Friend
{
    public enum FriendResponseRelationship
    {
        None,
        Banned,
        Recipient,
        Friend,
        Initiator,
        Error
    }

    public static class RelationshipMapper
    {
        public static FriendResponseRelationship Map(this EFriendRelationship relationship)
        {
            switch (relationship)
            {
                case EFriendRelationship.None:
                    return FriendResponseRelationship.None;
                case EFriendRelationship.Blocked:
                    return FriendResponseRelationship.Banned;
                case EFriendRelationship.RequestRecipient:
                    return FriendResponseRelationship.Recipient;
                case EFriendRelationship.Friend:
                    return FriendResponseRelationship.Friend;
                case EFriendRelationship.RequestInitiator:
                    return FriendResponseRelationship.Initiator;
                case EFriendRelationship.Ignored:
                    return FriendResponseRelationship.Banned;
                case EFriendRelationship.IgnoredFriend:
                    return FriendResponseRelationship.Banned;
                case EFriendRelationship.SuggestedFriend:
                    return FriendResponseRelationship.None;
                case EFriendRelationship.Max:
                    return FriendResponseRelationship.Error;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null);
            }
        }
    }
}
