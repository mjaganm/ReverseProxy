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
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class IngressWatcher : IHostedService, IDisposable
    {
        public IngressWatcher(IKubernetes client)
        {
            this.k8sClient = client;
        }

        private Timer timerWatcher { get; set; }

        private IKubernetes k8sClient { get; set; }

        private ManualResetEventSlim shutdownEvent { get; set; }

        public Task StartAsync(CancellationToken token)
        {
            timerWatcher = new Timer(this.StartWatcher, null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        private void StartWatcher(object state)
        {
            var ingresslistResp = this.k8sClient.ListIngressForAllNamespacesWithHttpMessagesAsync(watch: true);
            using (ingresslistResp.Watch<Extensionsv1beta1Ingress, Extensionsv1beta1IngressList>((type, item) =>
            {
                Console.WriteLine("==on watch event==");
                Console.WriteLine(type);
                Console.WriteLine(item.Metadata.Name);
                Console.WriteLine("==on watch event==");
            }))
            {
                this.shutdownEvent = new ManualResetEventSlim(false);

                this.shutdownEvent.Wait();
            }
        }

        public Task StopAsync(CancellationToken token)
        {
            this.shutdownEvent?.Set();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timerWatcher?.Dispose();
        }
    }
}
