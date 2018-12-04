using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Redis;


namespace ProxyServerTCP
{
    public static class CashController
    {
        //private static PooledRedisClientManager redisManager = new PooledRedisClientManager("localhost:6379");
        public static bool Save(string host, string key, string value)
        {
            bool isSuccess = false;

            using (RedisClient redisClient = new RedisClient(host))
            {
                if (redisClient.Get<string>(key) == null)
                {
                    isSuccess = redisClient.Set(key, value);
                }
            }
            return isSuccess;
        }

        public static string Get(string host, string key)
        {
            using (RedisClient redisClient = new RedisClient(host))
            {
                return redisClient.Get<string>(key);
            }
        }

        public static void Flush(String host)
        {
            using (RedisClient redisClient = new RedisClient(host))
            {
                //if(redisClient.HasConnected)
                try
                {
                    redisClient.FlushAll();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
