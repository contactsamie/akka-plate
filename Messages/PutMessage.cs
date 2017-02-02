namespace AkkaBootCampThings
{
    public class PutMessage : IUpdateRequestMessage, IActorMessage
    {
        public PutMessage(string id, Client client)
        {
            Client = client;
            Id = id;
        }

        public Client Client { get; }
        public string Id { get; }
    }
}