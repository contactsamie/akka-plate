namespace AkkaBootCampThings
{
    public class PostMessage : IUpdateRequestMessage, IActorMessage
    {
        public PostMessage(Client client, string id)
        {
            Client = client;
            Id = id;
        }

        public Client Client { get; }
        public string Id { get; }
    }
}