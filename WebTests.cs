#region

using Akka.Util.Internal;
using AkkaBootCampThings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SignalXLib.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
                    var data = await res.Content.ReadAsAsync<Client>();
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
                var products = await res2.Content.ReadAsAsync<List<Client>>();

                return products;
            }).Result as List<Client>;
            Debug.Assert(result != null, "result != null");
            foreach (var client in result)
                Assert.AreEqual(client.Address, client.ID);
        }

        [TestMethod]
        public void TestSomethingNew1()
        {
            var server = TestHelper.CreateServer(8018);
            var result = server(async (client, s, route, ui) =>
            {
                var api = "api/data/SendActorMessage/SendActorMessage?actorSelection=*/DataActor&messageJson={}&messageClassName=" + typeof(GetAllMessage).AssemblyQualifiedName;
                HttpResponseMessage res = null;
                var range = Enumerable.Range(0, 100);
                var enumerable = range as int[] ?? range.ToArray();
                foreach (var i in enumerable)
                {
                    res = await client.GetAsync(api);
                    res.EnsureSuccessStatusCode();
                }

                var products = await res.Content.ReadAsAsync<object>();
                return products;
            }).Result;
        }

        [TestMethod]
        public void TestSomethingNew2()
        {
            var server = TestHelper.CreateServer(8018);
            var result = server(async (client, s, route, ui) =>
            {
                HttpResponseMessage res = null;
                var range = Enumerable.Range(0, 100);
                var enumerable = range as int[] ?? range.ToArray();
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(ss => ss.GetTypes())
                    .Where(p => typeof(IActorMessage).IsAssignableFrom(p))
                    .Where(x => x.AssemblyQualifiedName != typeof(IActorMessage).AssemblyQualifiedName)
                    .ForEach(x =>
                    {
                        var api2 = "api/data/SendActorMessage/SendActorMessage?actorSelection=*/DataActor&messageJson={Id:123}&messageClassName=" + x.AssemblyQualifiedName;

                        Parallel.ForEach(enumerable, i =>
                        {
                            try
                            {
                                res = client.GetAsync(api2).Result;
                                res.EnsureSuccessStatusCode();
                                Debug.Assert(res != null, "res != null");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    });

                var products = await res.Content.ReadAsAsync<object>();
                return products;
            }).Result;
        }
    }
}