using Akka.Actor;
using System.Collections.Generic;
using System.Linq;

namespace AkkaBootCampThings
{
    public class DataQueryActor : ReceiveActor
    {
        private readonly Dictionary<string, Client> _data = new Dictionary<string, Client>();

        public DataQueryActor()
        {
            Receive<Client>(message =>
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