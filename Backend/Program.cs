using System;
using System.Configuration;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;

namespace Backend
{
    internal class Program
    {
        private static Config _clusterConfig;

        public static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection) ConfigurationManager.GetSection("akka");
            _clusterConfig = section.AkkaConfig;

            var port = args.Length > 0 ? args[0] : "0";

            var config =
                ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [backend]"))
                    .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config);
            system.ActorOf(Props.Create<Transformation>(), "backend");
            
            //starting 2 frontend nodes and 3 backend nodes
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}