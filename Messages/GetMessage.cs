namespace AkkaBootCampThings
{
    public class GetMessage : IQueryRequestMessage
    {
        public GetMessage(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}