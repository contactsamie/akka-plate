using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Akka.Actor;
using AkkaBootCampThings;
using SignalXLib.Lib;

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    public class DataController : ApiController
    {
        private readonly IUiNotificationService _notificationService;
        public DataController()
        {
            _notificationService=new SignalXNotificationService();
            ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
            if (ActorSystemThings.MyActorRef != null) return;
            SignalX.Server("Update", request =>
            {
                request.RespondToAll(request.Message);
            });
            ActorSystemThings.ActorSystem = ActorSystemThings.ActorSystem ?? ActorSystem.Create("MyActorSystem");
            ActorSystemThings.MyActorRef = ActorSystemThings.MyActorRef ??ActorSystemThings.ActorSystem.ActorOf(Props.Create(() => new DataActor(new DataService(_notificationService))),typeof (DataActor).Name);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> DoSomething(string id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, await ActorSystemThings.MyActorRef.Ask(id));
        }

        [HttpGet]
        public async Task<object> Get(string id)
        {
            var result = await ActorSystemThings.MyActorRef.Ask(new ActorMessages.GetMessage(id));
            return result;
        }

        [HttpGet]
        public async Task<IEnumerable<object>> Get()
        {
            var result =
                await ActorSystemThings.MyActorRef.Ask<List<DataRepo.Client>>(new ActorMessages.GetAllMessage());
            return result.ToArray();
        }

        [HttpPost]
        public Task Post([FromBody] DataRepo.Client client)
        {
            return ActorSystemThings.MyActorRef.Ask(new ActorMessages.PostMessage(client, client.ID));
        }

        [HttpPut]
        public Task Put(string id, [FromBody] DataRepo.Client editedClient)
        {
            return ActorSystemThings.MyActorRef.Ask(new ActorMessages.PutMessage(id, editedClient));
        }

        [HttpDelete]
        public Task Delete(string id)
        {
            return ActorSystemThings.MyActorRef.Ask(new ActorMessages.DeleteMessage(id));
        }
    }
}