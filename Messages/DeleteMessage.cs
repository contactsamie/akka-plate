namespace AkkaBootCampThings
{
    public class DeleteMessage : IUpdateRequestMessage, IActorMessage
    {
        public DeleteMessage(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}