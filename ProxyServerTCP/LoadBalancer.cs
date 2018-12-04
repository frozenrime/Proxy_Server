using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace ProxyServerTCP
{
    class LoadBalancer
    {
        private ConcurrentDictionary<IPEndPoint, int> serverPoint = new ConcurrentDictionary<IPEndPoint, int>();

        private static readonly Lazy<LoadBalancer> lazy =
            new Lazy<LoadBalancer>(() => new LoadBalancer());
        public static LoadBalancer Instance { get { return lazy.Value; } }

        private LoadBalancer()
        {
        }

        public void addServer(IPEndPoint p)
        {
            serverPoint.TryAdd(p, 0);
        }

        public IPEndPoint getServerPoint()
        {
            //var value = serverPoint.Where(x => x.Key != null).Min(x => x.Value);

            var rr = serverPoint.FirstOrDefault(x => x.Value == serverPoint.Values.Min());
            serverPoint.TryUpdate(rr.Key, (rr.Value + 1), rr.Value);


            return rr.Key;
        }

        public void remuveConnection(IPEndPoint p)
        {
            serverPoint[p] = serverPoint[p] - 1;
        }
    }
}
