#region

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

#endregion

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    [TestClass]
    public class WebTests
    {
        [TestMethod]
        public void TestMySync1Web()
        {
            TestMyWeb("http://localhost:8018/", "sync1");
        }

        [TestMethod]
        public void TestMySync1ToAsyncWebV2()
        {
            TestMyWeb("http://localhost:8228/", "Sync1ToAsyncV2");
        }

        public void TestMyWeb(string endpoint, string action)
        {
            Helper = new TestHelper(endpoint);

            Helper.StartWebApiServer(endpoint, () =>
            {
                var result = Helper.GetProducts(action).Result;
            });
        }

        public class ProductsController : ApiController
        {
            public ProductsController()
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;
            }

            [HttpGet]
            public HttpResponseMessage Sync1()
            {
                try
                {
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }

            [HttpGet]
            public Task<HttpResponseMessage> Sync1ToAsyncV2()
            {
                try
                {
                    return Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, true));
                }
                catch (Exception e)
                {
                    return Task.FromResult(Request.CreateResponse(HttpStatusCode.BadRequest));
                }
            }
        }

        #region SetUps
        private TestHelper Helper { set; get; }

        public class TestHelper
        {
            private readonly string _endpoint;

            public TestHelper(string endpoint = null)
            {
                _endpoint = endpoint;
            }

            private static HttpClient GetClient()
            {
                var requestHandler = new HttpClientHandler
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };

                return new HttpClient(requestHandler);
            }

            public async Task<IEnumerable<object>> GetProducts(string action)
            {
                ServicePointManager.MaxServicePointIdleTime = Timeout.Infinite;

                var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(10),
                    BaseAddress = new Uri(_endpoint)
                };

                var result = await client.GetAsync("api/products/" + action);
                result.EnsureSuccessStatusCode();
                var products = await result.Content.ReadAsAsync<IEnumerable<object>>().ConfigureAwait(false);

                return products;
            }

            public void StartWebApiServer(string endpoint, Action operation)
            {
                var config = new HttpSelfHostConfiguration(endpoint);
                config.Routes.MapHttpRoute("API Default", "api/{controller}/{action}/{id}", new
                {
                    id = RouteParameter.Optional
                });
                using (var server = new HttpSelfHostServer(config))
                {
                    server.OpenAsync().Wait();
                    operation();
                }
            }
        }

        #endregion
    }
}