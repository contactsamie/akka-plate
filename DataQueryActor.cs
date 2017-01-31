using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace AkkaBootCampThings
{
    public class DataQueryActor : ReceiveActor
    {
        private readonly Dictionary<string, DataRepo.Client> _data = new Dictionary<string, DataRepo.Client>();

        public DataQueryActor()
        {
            Receive<DataRepo.Client>(message =>
            {
                _data[message.ID] = message;
            });
            Receive<GetMessage>(message =>
            {
                Sender.Tell(_data[message.Id]);
            });
            Receive<GetAllMessage>(message =>
            {
                Sender.Tell(_data.Select(x => x.Value).ToList());
            });
        }
    }
}