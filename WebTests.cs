#region

using Akka.Actor;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using SignalXLib.Lib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.SelfHost;

#endregion

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    [TestClass]
    public class WebTests
    {
        private readonly TestHelper _testHelper = new TestHelper();

        [TestMethod]
        public void TestSomething()
        {
            var server = _testHelper.CreateServer(8018);
            var result = server(async (client, s, basePath, ui) =>
            {
                await ui(44111, UI, 1000000, (uiProcess) => { });
                var res = await client.GetAsync(basePath + "DoSomething/1");
                res.EnsureSuccessStatusCode();
                var products = await res.Content.ReadAsAsync<object>();
                return products;
            }).Result;
        }

        public class MyService : IService<object, object>
        {
            public object Execute(object message)
            {
                return message;
            }
        }

        public class MyActor : ReceiveActor
        {
            public MyActor(IService<object, object> service)
            {
                ReceiveAny(_ =>
                {
                    Sender.Tell(service.Execute(_));
                });
            }
        }

        public interface IService<T, TR>
        {
            TR Execute(T message);
        }

        public class ProductsController : ApiController
        {
            [HttpGet]
            public async Task<HttpResponseMessage> DoSomething(int id)
            {
                CreateActorSystem("MyActorSystem");
                return Request.CreateResponse(HttpStatusCode.OK, await MyActorRef.Ask(id));
            }

            public void CreateActorSystem(string serverActorSystemName, string actorSystemConfig = null)
            {
                if (MyActorRef == null)
                {
                    MyActorRef = (string.IsNullOrEmpty(actorSystemConfig)
                                       ? ActorSystem.Create(serverActorSystemName)
                                       : ActorSystem.Create(serverActorSystemName, actorSystemConfig))
                                       .ActorOf(Props.Create(() => new MyActor(new MyService())));
                }
            }

            public IActorRef MyActorRef { get; set; }
                
            public ProductsController()
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
            }
        }
        
       
        public class DataController : ApiController
        {
            public IEnumerable<object> Get()
            {
                ClientFilter filter = GetFilter();

                var result =  new List<Client> {
                new Client {
                    Name = "Otto Clay",
                    Age = 61,
                    Country = Country.Canada,
                    Address = "Ap #897-1459 Quam Avenue",
                    Married = false
                },
                new Client {
                    Name = "Lacey Hess",
                    Age = 29,
                    Country = Country.Russia,
                    Address = "Ap #365-8835 Integer St.",
                    Married = false
                },
                new Client {
                    Name = "Timothy Henson",
                    Age = 78,
                    Country = Country.UnitedStates,
                    Address = "911-5143 Luctus Ave",
                    Married = false
                },
                new Client {
                    Name = "Ramona Benton",
                    Age = 43,
                    Country = Country.Brazil,
                    Address = "Ap #614-689 Vehicula Street",
                    Married = true
                },
                new Client {
                    Name = "Ezra Tillman",
                    Age = 51,
                    Country = Country.UnitedStates,
                    Address = "P.O. Box 738, 7583 Quisque St.",
                    Married = true
                },
                new Client {
                    Name = "Dante Carter",
                    Age = 59,
                    Country = Country.UnitedStates,
                    Address = "P.O. Box 976, 6316 Lorem, St.",
                    Married = false
                },
                new Client {
                    Name = "Christopher Mcclure",
                    Age = 58,
                    Country = Country.UnitedStates,
                    Address = "847-4303 Dictum Av.",
                    Married = true
                },
                new Client {
                    Name = "Ruby Rocha",
                    Age = 62,
                    Country = Country.Canada,
                    Address = "5212 Sagittis Ave",
                    Married = false
                },
                new Client {
                    Name = "Imelda Hardin",
                    Age = 39,
                    Country = Country.Brazil,
                    Address = "719-7009 Auctor Av.",
                    Married = false
                },
                new Client {
                    Name = "Jonah Johns",
                    Age = 28,
                    Country = Country.Brazil,
                    Address = "P.O. Box 939, 9310 A Ave",
                    Married = false
                },
                new Client {
                    Name = "Herman Rosa",
                    Age = 49,
                    Country = Country.Russia,
                    Address = "718-7162 Molestie Av.",
                    Married = true
                },
                new Client {
                    Name = "Arthur Gay",
                    Age = 20,
                    Country = Country.Russia,
                    Address = "5497 Neque Street",
                    Married = false
                },
                new Client {
                    Name = "Xena Wilkerson",
                    Age = 63,
                    Country = Country.UnitedStates,
                    Address = "Ap #303-6974 Proin Street",
                    Married = true
                },
                new Client {
                    Name = "Lilah Atkins",
                    Age = 33,
                    Country = Country.Brazil,
                    Address = "622-8602 Gravida Ave",
                    Married = true
                }
            };

                return result.ToArray();
            }

            private ClientFilter GetFilter()
            {
                NameValueCollection filter = HttpUtility.ParseQueryString(Request.RequestUri.Query);

                return new ClientFilter
                {
                    Name = filter["Name"],
                    Address = filter["Address"],
                    Country = (filter["Country"] == "0") ? (Country?)null : (Country)int.Parse(filter["Country"]),
                    Married = String.IsNullOrEmpty(filter["Married"]) ? (bool?)null : bool.Parse(filter["Married"])
                };
            }

            public void Post([FromBody]Client client)
            {
               
            }


            public void Put(int id, [FromBody]Client editedClient)
            {
              
              

                
            }

            public void Delete(int id)
            {
              
                
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

            private const string Route = "api/{controller}/{id}";

            public Func<Func<HttpClient, HttpSelfHostServer, string, Func<int, string, int, Action<Process>, Task<object>>, Task<object>>, Task<object>> CreateServer(int serverPort, string domain = "http://localhost")
            {
                return async (m) =>
                                {
                                    var serverEndpoint = domain + ":" + serverPort + "/";

                                    const string baseLink = "api/products/";
                                    var config = new HttpSelfHostConfiguration(serverEndpoint);
                                    config.Routes.MapHttpRoute("API Default", Route, new { id = RouteParameter.Optional });
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
                                             return true;
                                         });
                                    }
                                };
            }
        }

        private string UI = @"<!DOCTYPE html>
							<html>
                            <head>
                               <script src='https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js'></script>
							<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js'></script>
						    <script src='https://unpkg.com/signalx'></script>
                            <script src='https://ajax.googleapis.com/ajax/libs/angularjs/1.5.6/angular.min.js'></script>
                            <script src='https://ajax.googleapis.com/ajax/libs/angularjs/1.5.6/angular-route.js'></script>                          
                            <link type = 'ext/css' rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid.min.css' />
                            <link type = 'text/css' rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid-theme.min.css' />
                            <script type = 'text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid.min.js'></script>                          
                            </head>
							<body >
                            <div ng-app='all' >
							<div  ng-controller='ActorsCtrl'>
							    <input ng-model='inp' type='text'/>
							    <button ng-click='inp=inp+1'>Send Message To Server</button>
                             
                            </div> 
                           </div>
                           <div id='jsGrid'></div>
							
							<script>
							    signalx.debug(function (o) { console.log(o); });
                                signalx.error(function (o) { console.log(o); });
                                signalx.ready(function (server) {
                                    var app = angular.module('all', ['ngRoute']);
                            
                                   app.controller('ActorsCtrl', function ($scope, $rootScope, $http, $q, $timeout) {
      
                                    });
                                    $(function () {
                                            var countries = [
                                                { Name: '', Id: 0 },
                                                { Name: 'United States', Id: 1 },
                                                { Name: 'Canada', Id: 2 },
                                                { Name: 'United Kingdom', Id: 3 },
                                                { Name: 'France', Id: 4 },
                                                { Name: 'Brazil', Id: 5 },
                                                { Name: 'China', Id: 6 },
                                                { Name: 'Russia', Id: 7 }
                                            ];

                                            $('#jsGrid').jsGrid({
                                                height: '50%',
                                                width: '100%',

                                                filtering: true,
                                                inserting: true,
                                                editing: true,
                                                sorting: true,
                                                paging: true,
                                                autoload: true,

                                                pageSize: 10,
                                                pageButtonCount: 5,

                                                deleteConfirm: 'Do you really want to delete client?',

                                                controller: {
                                                    loadData: function (filter) {
                                                        return $.ajax({
                                                            type: 'GET',
                                                            url:'http://localhost:8018/api/data/',
                                                            data: filter,
                                                            dataType: 'json'
                                                        });
                                                    },

                                                    insertItem: function (item) {
                                                        return $.ajax({
                                                            type: 'POST',
                                                            url: 'http://localhost:8018/api/data/',
                                                            data: item,
                                                            dataType: 'json'
                                                        });
                                                    },

                                                    updateItem: function (item) {
                                                        return $.ajax({
                                                            type: 'PUT',
                                                            url: 'http://localhost:8018/api/data/'+ item.ID,
                                                            data: item,
                                                            dataType: 'json'
                                                        });
                                                    },

                                                    deleteItem: function (item) {
                                                        return $.ajax({
                                                            type: 'DELETE',
                                                            url: 'http://localhost:8018/api/data/' + item.ID,
                                                            dataType: 'json'
                                                        });
                                                    }
                                                },

                                                fields: [
                                                    { name: 'Name', type: 'text', width: 150 },
                                                    { name: 'Age', type: 'number', width: 50, filtering: false },
                                                    { name: 'Address', type: 'text', width: 200 },
                                                    { name: 'Country', type: 'select', items: countries, valueField: 'Id', textField: 'Name' },
                                                    { name: 'Married', type: 'checkbox', title: 'Is Married', sorting: false },
                                                    { type: 'control' }
                                                ]
                                            });
                                        });
                                });					   
							</script>  
                       
							</body>
							</html>";
    }


    public enum Country
    {
        UnitedStates = 1,
        Canada = 2,
        UnitedKingdom = 3,
        France = 4,
        Brazil = 5,
        China = 6,
        Russia = 7
    }

    public class ClientFilter
    {

        public string Name { get; set; }
        public string Address { get; set; }
        public Country? Country { get; set; }
        public bool? Married { get; set; }

    }

    public class Client
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Country? Country { get; set; }
        public string Address { get; set; }
        public bool Married { get; set; }

    }
}