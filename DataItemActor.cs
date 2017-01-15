using Akka.Actor;

namespace AkkaBootCampThings
{
    public class DataItemActor : ReceiveActor
    {
        private readonly string _id;
        private readonly IActorRef _queryActor;
        private DataRepo.Client _selfData;

        public DataItemActor(IDataService<object, object> service, string productId, IActorRef dataQueryActor)
        {
            _queryActor = dataQueryActor;
            _id = productId;
            LoadData(service);
                
            Receive<ActorMessages.PostMessage>(message =>
            {
                Sender.Tell(service.Post(message, message.Client));
                LoadData(service);
            });
            Receive<ActorMessages.PutMessage>(message =>
            {
                Sender.Tell(service.Put(message, message.Client, message.Id));
                LoadData(service);
            });
            Receive<ActorMessages.DeleteMessage>(message =>
            {
                Sender.Tell(service.Delete(message, message.Id));
                LoadData(service);
            });
        }

        private void LoadData(IDataService<object, object> service)
        {
            _selfData = service.Get(null, _id);
            _queryActor.Tell(_selfData);
        }
    }
}