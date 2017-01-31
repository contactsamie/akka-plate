namespace AkkaBootCampThings
{
    public interface IActorMessage
    {
    }

    public class GetAllMessage : IQueryRequestMessage, IActorMessage
    {

    }
    public interface IQueryRequestMessage
    {

    }

    public interface IUpdateRequestMessage
    {
        string Id { get; }
    }

    public class GetMessage : IQueryRequestMessage
    {
        public GetMessage(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class PostMessage : IUpdateRequestMessage, IActorMessage
    {
        public PostMessage(DataRepo.Client client, string id)
        {
            Client = client;
            Id = id;
        }

        public DataRepo.Client Client { get; }
        public string Id { get; }
    }

    public class PutMessage : IUpdateRequestMessage, IActorMessage
    {
        public PutMessage(string id, DataRepo.Client client)
        {
            Client = client;
            Id = id;
        }

        public DataRepo.Client Client { get; }
        public string Id { get; }
    }

    public class DeleteMessage : IUpdateRequestMessage, IActorMessage
    {
        public DeleteMessage(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class DoSomethingMessage: IActorMessage
    {
    }
    public class InitializeInventoriesFromStorageMessage: IActorMessage
    {
    }

}