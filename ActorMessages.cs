namespace AkkaBootCampThings
{
    public class ActorMessages
    {
        public class GetAllMessage: IQueryRequestMessage
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

        public class PostMessage : IUpdateRequestMessage
        {
            public PostMessage(DataRepo.Client client, string id)
            {
                Client = client;
                Id = id;
            }

            public DataRepo.Client Client { get; }
            public string Id { get; }
        }

        public class PutMessage : IUpdateRequestMessage
        {
            public PutMessage(string id, DataRepo.Client client)
            {
                Client = client;
                Id = id;
            }

            public DataRepo.Client Client { get; }
            public string Id { get; }
        }

        public class DeleteMessage : IUpdateRequestMessage
        {
            public DeleteMessage(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        public class DoSomethingMessage
        {
        }public class InitializeInventoriesFromStorageMessage
        {
        }
    }
}