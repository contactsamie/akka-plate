#region

using Akka.Actor;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Owin;
using SignalXLib.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

#endregion

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    [TestClass]
    public class WebTests
    {
        private readonly TestHelper _testHelper = new WebTests.TestHelper();

        [TestMethod]
        public void TestSomething()
        {
            var server = _testHelper.CreateServer<bool>(8018);
            var result = server(async (client, s, basePath, ui) =>
            {
                await ui(44111, _uiClass.UI, 1000000, (uiProcess) => { });
                return true;
            }).Result;
        }

        [TestMethod]
        public void TestSomething2()
        {
            var server = _testHelper.CreateServer<object>(8018);
            var result = server(async (client, s, basePath, ui) =>
            {
                var res = await client.GetAsync(basePath + "DoSomething/1");
                res.EnsureSuccessStatusCode();
                var products = await res.Content.ReadAsAsync<object>();
                return products;
            }).Result;
        }

        [TestMethod]
        public void TestSomething3()
        {
            var server = _testHelper.CreateServer<List<DataRepo.Client>>(8018);
            var result = server(async (client, s, basePath, ui) =>
            {
                var tasks = Enumerable.Range(1, 1000).Select(async i =>
                {
                    var res = await client.GetAsync(basePath + "Get/" + i);
                    res.EnsureSuccessStatusCode();
                    var data = await res.Content.ReadAsAsync<DataRepo.Client>();
                    data.Address = i.ToString();
                    var jsonString = JsonConvert.SerializeObject(data);
                    HttpContent httpContent = new StringContent(jsonString);
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    //add the header with the access token
                    //hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    await client.PutAsync(basePath + "Put/" + i, httpContent);
                    // some post stuff
                });
                await Task.WhenAll(tasks);

                List<DataRepo.Client> products = new List<DataRepo.Client>();
                var res2 = await client.GetAsync(basePath + "Get");
                res2.EnsureSuccessStatusCode();
                products = await res2.Content.ReadAsAsync<List<DataRepo.Client>>();

                return products;
            }).Result;
            foreach (var client in result)
            {
                Assert.AreEqual(client.Address, client.ID.ToString());
            }
        }

        public class MyService : WebTests.IService<object, object>
        {
            public object Execute(object message)
            {
                return message;
            }
        }

        public interface IService<T, TR>
        {
            TR Execute(T message);
        }

        public class DataActor : ReceiveActor
        {
            private static DataRepo _dataRepo;

            public DataActor(IService<object, object> service)
            {
                _dataRepo = _dataRepo ?? new DataRepo();

                Receive<ActorMessages.DoSomethingMessage>(message =>
               {
                   SignalX.RespondToAll("Update", message);
                   Sender.Tell(service.Execute(message));
               });
                Receive<ActorMessages.GetAllMessage>(message =>
                {
                    SignalX.RespondToAll("Update", message);
                    Sender.Tell(_dataRepo.Result);
                });
                Receive<ActorMessages.GetMessage>(message =>
                {
                    SignalX.RespondToAll("Update", message);
                    Sender.Tell(_dataRepo.Result.FirstOrDefault(x => x.ID == message.Id));
                });
                Receive<ActorMessages.PostMessage>(message =>
                {
                    SignalX.RespondToAll("Update", message);
                    _dataRepo.Result.Add(message.Client);
                    Sender.Tell(true);
                });
                Receive<ActorMessages.PutMessage>(message =>
                {
                    SignalX.RespondToAll("Update", message);
                    var index = _dataRepo.Result.FindIndex(x => x.ID == message.Id);
                    message.Client.ID = message.Id;
                    _dataRepo.Result[index] = message.Client;
                    Sender.Tell(true);
                });
                Receive<ActorMessages.DeleteMessage>(message =>
                {
                    SignalX.RespondToAll("Update", message);
                    _dataRepo.Result.Remove(_dataRepo.Result.Find(x => x.ID == message.Id));
                    Sender.Tell(true);
                });
            }
        }

        public class DataController : ApiController
        {
            public DataController()
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
                _actorSystemHelper = _actorSystemHelper ?? new ActorSystemHelper();
                if (_actorSystemHelper.MyActorRef == null)
                {
                    _actorSystemHelper.CreateActorAndSystem(() => new DataActor(new MyService()), "MyActorSystem");
                    SignalX.Server("Update", (request) =>
                    {
                        request.RespondToAll(request.Message);
                    });
                }
            }

            private static WebTests.ActorSystemHelper _actorSystemHelper;

            [HttpGet]
            public async Task<HttpResponseMessage> DoSomething(int id)
            {
                return Request.CreateResponse(HttpStatusCode.OK, await _actorSystemHelper.MyActorRef.Ask(id));
            }

            [HttpGet]
            public async Task<object> Get(int id)
            {
                var result = await _actorSystemHelper.MyActorRef.Ask(new ActorMessages.GetMessage(id));
                return result;
            }

            [HttpGet]
            public async Task<IEnumerable<object>> Get()
            {
                var result = await _actorSystemHelper.MyActorRef.Ask<List<DataRepo.Client>>(new ActorMessages.GetAllMessage());
                return result.ToArray();
            }

            [HttpPost]
            public Task Post([FromBody]DataRepo.Client client)
            {
                return _actorSystemHelper.MyActorRef.Ask(new ActorMessages.PostMessage(client));
            }

            [HttpPut]
            public Task Put(int id, [FromBody]DataRepo.Client editedClient)
            {
                return _actorSystemHelper.MyActorRef.Ask(new ActorMessages.PutMessage(id, editedClient));
            }

            [HttpDelete]
            public Task Delete(int id)
            {
                return _actorSystemHelper.MyActorRef.Ask(new ActorMessages.DeleteMessage(id));
            }
        }

        public class TestHelper
        {
            public class UiStartup
            {
                public void Configuration(IAppBuilder app)
                {
                    app.UseSignalX(new SignalX(""));
                }
            }

            //public class CustomHeaderHandler : DelegatingHandler
            //{
            //    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            //    {
            //        return base.SendAsync(request, cancellationToken)
            //            .ContinueWith((task) =>
            //            {
            //                HttpResponseMessage response = task.Result;
            //                //response.Headers.Add("Access-Control-Allow-Origin", "*");
            //                //response.Headers.Add("Cache-Control", "no-cache");
            //                //response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            //                //response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            //                //response.Headers.Add("Access-Control-Max-Age", "1728000");
            //                return response;
            //            });
            //    }
            //}

            private const string Route = "api/{controller}/{action}/{id}";

            public Func<Func<HttpClient, HttpSelfHostServer, string, Func<int, string, int, Action<Process>, Task<T>>, Task<T>>, Task<T>> CreateServer<T>(int serverPort, string domain = "http://localhost")
            {
                return async (m) =>
                                {
                                    var serverEndpoint = domain + ":" + serverPort + "/";

                                    const string baseLink = "api/data/";
                                    var config = new HttpSelfHostConfiguration(serverEndpoint);
                                    config.Routes.MapHttpRoute("API Default", Route, new { id = RouteParameter.Optional });
                                    //config.MessageHandlers.Add(new CustomHeaderHandler());
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

                                        return await m(client, server, baseLink, async (uiPort, uiString, delayMs, uiExec) =>
                                         {
                                             var uiEndpoint = domain + ":" + uiPort + "/";
                                             using (WebApp.Start<UiStartup>(uiEndpoint))
                                             {
                                                 var filePath = AppDomain.CurrentDomain.BaseDirectory + "\\index.html";
                                                 File.WriteAllText(filePath, uiString);
                                                 var proc = Process.Start(uiEndpoint);
                                                 await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
                                                 uiExec(proc);
                                             }
                                             //todo fix this
                                             return default(T);
                                         });
                                    }
                                };
            }
        }

        public class ActorSystemHelper
        {
            public IActorRef MyActorRef { get; set; }

            public void CreateActorAndSystem<T>(Expression<Func<T>> actorFunc, string serverActorSystemName, SupervisorStrategy supervisorStrategy = null, string actorSystemConfig = null) where T : ActorBase
            {
                if (MyActorRef == null)
                {
                    MyActorRef = (string.IsNullOrEmpty(actorSystemConfig)
                            ? ActorSystem.Create(serverActorSystemName)
                            : ActorSystem.Create(serverActorSystemName, actorSystemConfig))
                        .ActorOf(Props.Create(actorFunc, supervisorStrategy), typeof(T).Name);
                }
            }
        }

        private readonly UIClass _uiClass = new UIClass();

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