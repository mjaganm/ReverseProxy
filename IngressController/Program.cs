/**
 *
 * MIT License
 *
 * Copyright (c) 2018 Jagan Mohan Maddukuri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * */

namespace ReverseProxy.IngressController
{
    using k8s;
    using k8s.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            using (var host = new HostBuilder()
                //.ConfigureLogging((logging) => { logging.AddConsole(); })
                .ConfigureServices((hostBuilderContext, services) =>
                {
                    // Load from in-cluster configuration:
                    // var config = KubernetesClientConfiguration.InClusterConfig()
                    var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:8001" };
                    services.AddSingleton(config);

                    // Setup the http client
                    services.AddHttpClient("K8s")
                    .AddTypedClient<IKubernetes>((httpClient, serviceProvider) =>
                    {
                        return new Kubernetes(
                            serviceProvider.GetRequiredService<KubernetesClientConfiguration>(),
                            httpClient);
                    })
                    .ConfigurePrimaryHttpMessageHandler(config.CreateDefaultHttpClientHandler)
                    .AddHttpMessageHandler(KubernetesClientConfiguration.CreateWatchHandler);

                    // Add the class that uses the client
                    services.AddHostedService<PodWatcher>();
                    services.AddHostedService<ServiceWatcher>();
                    services.AddHostedService<IngressWatcher>();
                })
                .Build())
            {
                await host.StartAsync().ConfigureAwait(false);

                await Task.Delay(Timeout.InfiniteTimeSpan);

                await host.StopAsync().ConfigureAwait(false);
            }

            Console.WriteLine("Hello World!");
        }
    }
}
