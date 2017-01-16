using System.Collections.Generic;
using Akka.Actor;

namespace AkkaBootCampThings
{
    public class DataActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _dataItems;
        private readonly IDataService<object, object> _service;
        private readonly IActorRef _dataQueryActor;

        public DataActor(IDataService<object, object> service)
        {
            _dataItems = new Dictionary<string, IActorRef>();
            _service = service;
            _dataQueryActor = Context.ActorOf(Props.Create(() => new DataQueryActor()));
            Become(Initializing);
        }

        private void Initializing()
        {
            Self.Tell(new ActorMessages.InitializeInventoriesFromStorageMessage());

            Receive<ActorMessages.InitializeInventoriesFromStorageMessage>(message =>
            {
                var dataList = _service.GetAll(message);
                    
                foreach (var s in dataList)
                {
                    GetOrActorRef(s.ID.ToString());
                }
                Become(Processing);
            });
        }

        private void Processing()
        {
            Receive<ActorMessages.DoSomethingMessage>(message =>
            {
                Sender.Tell(_service.DoSomething(message));
            });
                
            Receive<ActorMessages.IQueryRequestMessage>(message =>
            {
                _dataQueryActor.Forward(message);
            });
            Receive<ActorMessages.IUpdateRequestMessage>(message =>
            {
                var actorRef = GetOrActorRef(message.Id.ToString());
                actorRef.Forward(message);
            });
        }

        private IActorRef GetOrActorRef(string productId)
        {
            if (_dataItems.ContainsKey(productId)) return _dataItems[productId];

            var productActorRef =Context.ActorOf(Props.Create(() => new DataItemActor(_service, productId, _dataQueryActor)),"DataItemActor_" + productId);
            _dataItems.Add(productId, productActorRef);
            return _dataItems[productId];
        }
    }
}