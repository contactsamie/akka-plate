#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;
using Akka.Actor;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Owin;
using SignalXLib.Lib;

#endregion

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    [TestClass]
    public class WebTests
    {
        [TestMethod]
        public void TestSomething()
        {
            var server = TestHelper.CreateServer(8018);
            server(async (client, s, route, ui) =>
            {
                await
                    ui(44111, UIClass.UI, null, 1000000, uiProcess => { }, app => { app.UseSignalX(new SignalX("")); });
                return true;
            }).Wait();
        }

        [TestMethod]
        public void TestSomething2()
        {
            var server = TestHelper.CreateServer(8018);
            var result = server(async (client, s, route, ui) =>
            {
                var res = await client.GetAsync("api/data/" + "DoSomething/1");
                res.EnsureSuccessStatusCode();
                var products = await res.Content.ReadAsAsync<object>();
                return products;
            }).Result;
        }

        [TestMethod]
        public void TestSomething3()
        {
            var server = TestHelper.CreateServer(8018);
            var result = server(async (client, s, route, ui) =>
            {
                var tasks = Enumerable.Range(1, 1000).Select(async i =>
                {
                    var res = await client.GetAsync("api/data/Get/" + i);
                    res.EnsureSuccessStatusCode();
                    var data = await res.Content.ReadAsAsync<DataRepo.Client>();
                    data.Address = i.ToString();
                    var jsonString = JsonConvert.SerializeObject(data);
                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    //add the header with the access token
                    //hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    await client.PutAsync("api/data/Put/" + i, httpContent);
                    // some post stuff
                });
                await Task.WhenAll(tasks);

                var res2 = await client.GetAsync("api/data/Get");
                res2.EnsureSuccessStatusCode();
                var products = await res2.Content.ReadAsAsync<List<DataRepo.Client>>();

                return products;
            }).Result as List<DataRepo.Client>;
            Debug.Assert(result != null, "result != null");
            foreach (var client in result)
            {
                Assert.AreEqual(client.Address, client.ID.ToString());
            }
        }

        public class DataService : IDataService<object, object>
        {
            private static DataRepo _dataRepo;

            public DataService()
            {
                _dataRepo = _dataRepo ?? new DataRepo();
            }

            public object DoSomething(object message)
            {
                SignalX.RespondToAll("Update", message);
                return message;
            }

            public List<DataRepo.Client> GetAll(object message)
            {
                SignalX.RespondToAll("GetAll", message);
                return _dataRepo.Result;
            }

            public DataRepo.Client Get(object message, int id)
            {
                SignalX.RespondToAll("Get", message);
                return _dataRepo.Result.FirstOrDefault(x => x.ID == id);
            }

            public bool Post(object message, DataRepo.Client data)
            {
                SignalX.RespondToAll("Post", message);
                _dataRepo.Result.Add(data);
                return true;
            }

            public bool Put(object message, DataRepo.Client data, int id)
            {
                SignalX.RespondToAll("Put", message);
                var index = _dataRepo.Result.FindIndex(x => x.ID == id);
                data.ID = id;
                _dataRepo.Result[index] = data;

                return true;
            }

            public bool Delete(object message, int id)
            {
                SignalX.RespondToAll("Delete", message);
                _dataRepo.Result.Remove(_dataRepo.Result.Find(x => x.ID == id));
                return true;
            }
        }

        public interface IDataService<T, TR>
        {
            TR DoSomething(T message);
            List<DataRepo.Client> GetAll(object message);
            DataRepo.Client Get(object message, int id);
            bool Post(object message, DataRepo.Client data);
            bool Put(object message, DataRepo.Client data, int id);
            bool Delete(object message, int id);
        }

        public class DataActor : ReceiveActor
        {
            public DataActor(IDataService<object, object> service)
            {
                Receive<ActorMessages.DoSomethingMessage>(message => { Sender.Tell(service.DoSomething(message)); });

                Receive<ActorMessages.GetAllMessage>(message => { Sender.Tell(service.GetAll(message)); });
                Receive<ActorMessages.GetMessage>(message => { Sender.Tell(service.Get(message, message.Id)); });

                Receive<ActorMessages.PostMessage>(message => { Sender.Tell(service.Post(message, message.Client)); });
                Receive<ActorMessages.PutMessage>(message => { Sender.Tell(service.Put(message, message.Client, message.Id)); });
                Receive<ActorMessages.DeleteMessage>(message => { Sender.Tell(service.Delete(message, message.Id)); });
            }
        }

        public class DataController : ApiController
        {
            private static ActorSystem _actorSystem;
            private static IActorRef _myActorRef;

            public DataController()
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
                if (_myActorRef != null) return;
                SignalX.Server("Update", request => { request.RespondToAll(request.Message); });
                _actorSystem = _actorSystem ?? ActorSystem.Create("MyActorSystem");
                _myActorRef = _myActorRef ??_actorSystem.ActorOf(Props.Create(() => new DataActor(new DataService())),typeof (DataActor).Name);
            }

            [HttpGet]
            public async Task<HttpResponseMessage> DoSomething(int id)
            {
                return Request.CreateResponse(HttpStatusCode.OK, await _myActorRef.Ask(id));
            }

            [HttpGet]
            public async Task<object> Get(int id)
            {
                var result = await _myActorRef.Ask(new ActorMessages.GetMessage(id));
                return result;
            }

            [HttpGet]
            public async Task<IEnumerable<object>> Get()
            {
                var result = await _myActorRef.Ask<List<DataRepo.Client>>(new ActorMessages.GetAllMessage());
                return result.ToArray();
            }

            [HttpPost]
            public Task Post([FromBody] DataRepo.Client client)
            {
                return _myActorRef.Ask(new ActorMessages.PostMessage(client));
            }

            [HttpPut]
            public Task Put(int id, [FromBody] DataRepo.Client editedClient)
            {
                return _myActorRef.Ask(new ActorMessages.PutMessage(id, editedClient));
            }

            [HttpDelete]
            public Task Delete(int id)
            {
                return _myActorRef.Ask(new ActorMessages.DeleteMessage(id));
            }
        }

        public class TestHelper
        {
            public static
                Func<Func<HttpClient, HttpSelfHostServer, string,Func<int, string, string, int, Action<Process>, Action<IAppBuilder>, Task<object>>, Task<object>>, Task<object>> CreateServer(int serverPort,string domain = "http://localhost", string Route = "api/{controller}/{action}/{id}")
            {
                return async m =>
                {
                    var serverEndpoint = domain + ":" + serverPort + "/";
                    var config = new HttpSelfHostConfiguration(serverEndpoint);
                    config.Routes.MapHttpRoute("API Default", Route, new {id = RouteParameter.Optional});
                    var cors = new EnableCorsAttribute("*", "*", "*");
                    config.EnableCors(cors);
                    using (var server = new HttpSelfHostServer(config))
                    {
                        await server.OpenAsync();

                        ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
                        var client = new HttpClient
                        {
                            Timeout = TimeSpan.FromMinutes(10),
                            BaseAddress = new Uri(serverEndpoint)
                        };

                        return
                            await
                                m(client, server, Route,
                                    async (uiPort, uiString, uiFilePath, delayMs, uiExec, uiAppBuilder) =>
                                    {
                                        uiFilePath = string.IsNullOrEmpty(uiFilePath)
                                            ? AppDomain.CurrentDomain.BaseDirectory + "\\index.html"
                                            : uiFilePath;
                                        var uiEndpoint = domain + ":" + uiPort + "/";
                                        using (WebApp.Start(uiEndpoint, uiAppBuilder))
                                        {
                                            if (uiString != null)
                                            {
                                                File.WriteAllText(uiFilePath, uiString);
                                            }
                                            var proc = Process.Start(uiEndpoint);
                                            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
                                            uiExec(proc);
                                        }
                                        //todo fix this
                                        return null;
                                    });
                    }
                };
            }
        }

        public class ActorMessages
        {
            public class GetAllMessage
            {
            }

            public class GetMessage
            {
                public GetMessage(int id)
                {
                    Id = id;
                }

                public int Id { get; }
            }

            public class PostMessage
            {
                public PostMessage(DataRepo.Client client)
                {
                    Client = client;
                }

                public DataRepo.Client Client { get; }
            }

            public class PutMessage
            {
                public PutMessage(int id, DataRepo.Client client)
                {
                    Client = client;
                    Id = id;
                }

                public DataRepo.Client Client { get; }
                public int Id { get; }
            }

            public class DeleteMessage
            {
                public DeleteMessage(int id)
                {
                    Id = id;
                }

                public int Id { get; }
            }

            public class DoSomethingMessage
            {
            }
        }
    }
}