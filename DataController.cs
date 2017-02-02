using Akka.Actor;
using AkkaBootCampThings;
using Newtonsoft.Json;
using SignalXLib.Lib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    public class DataController : ApiController
    {
        private readonly IUiNotificationService _notificationService;

        public DataController()
        {
            _notificationService = new SignalXNotificationService();
            ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
            if (ActorSystemThings.MyActorRef != null) return;
            SignalX.Server("Update", request =>
            {
                request.RespondToAll(request.Message);
            });
            ActorSystemThings.ActorSystem = ActorSystemThings.ActorSystem ?? ActorSystem.Create("MyActorSystem");
            ActorSystemThings.MyActorRef = ActorSystemThings.MyActorRef ?? ActorSystemThings.ActorSystem.ActorOf(Props.Create(() => new DataActor(new DataService(_notificationService))), typeof(DataActor).Name);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> SendActorMessage([FromUri]string actorSelection, [FromUri]string messageClassName = null, [FromUri] string messageJson = null, [FromUri] int maxWaitSeconds = 10)
        {
            try
            {
                if (actorSelection == null) throw new ArgumentNullException(nameof(actorSelection));
                var messageToSend = string.IsNullOrEmpty(messageClassName) ?
                messageJson :
                JsonConvert.DeserializeObject(messageJson ?? "{}", Type.GetType(messageClassName, true, true));
                var actor = ActorSystemThings.ActorSystem.ActorSelection(actorSelection);
                var actorRef = await actor.ResolveOne(TimeSpan.FromSeconds(3));
                var result = await actorRef.Ask(messageToSend, TimeSpan.FromSeconds(maxWaitSeconds));
                return Request.CreateResponse(result);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Actor " + actorSelection + " is not alive i.e failed : " + e.Message + " - " + e.InnerException?.Message, e);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> DoSomething(string id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, await ActorSystemThings.MyActorRef.Ask(id));
        }

        [HttpGet]
        public Task<Client> Get(string id)
        {
            return ActorSystemThings.MyActorRef.Ask<Client>(new GetMessage(id));
        }

        [HttpGet]
        public Task<List<Client>> Get()
        {
            return ActorSystemThings.MyActorRef.Ask<List<Client>>(new GetAllMessage());
        }

        [HttpPost]
        public Task Post([FromBody] Client client)
        {
            return ActorSystemThings.MyActorRef.Ask(new PostMessage(client, client.ID));
        }

        [HttpPut]
        public Task Put(string id, [FromBody] Client editedClient)
        {
            return ActorSystemThings.MyActorRef.Ask(new PutMessage(id, editedClient));
        }

        [HttpDelete]
        public Task Delete(string id)
        {
            return ActorSystemThings.MyActorRef.Ask(new DeleteMessage(id));
        }
    }
}