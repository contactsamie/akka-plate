using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace AkkaBootCampThings
{
    public class TestHelper
    {
        public static
            Func<
                Func<
                     HttpClient
                    , HttpSelfHostServer
                    , string
                    , Func<int, string, string, int, Action<Process>, Action<IAppBuilder>, Task<object>>
                    , Task<object>>
                , Task<object>> CreateServer(
            int serverPort
            , string domain = "http://localhost"
            , string Route = "api/{controller}/{action}/{id}")
        {
            return async m =>
            {
                var serverEndpoint = domain + ":" + serverPort + "/";
                var config = new HttpSelfHostConfiguration(serverEndpoint);
                config.Routes.MapHttpRoute("API Default", Route, new { id = RouteParameter.Optional });
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
}