namespace DotaClient.Friend
{
    public class FriendResponseContainer
    {
        public FriendResponseRelationship Relationship { get; }
        public int StatusCode { get; }
        public FriendResponseStatus Status { get; }

        public FriendResponseContainer(int statusCode, FriendResponseRelationship relationship, FriendResponseStatus status)
        {
            StatusCode = statusCode;
            Relationship = relationship;
            Status = status;
        }
    }
}
