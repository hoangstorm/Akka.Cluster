using System;
using System.Configuration;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Util.Internal;
using log4net;
using Shared;

namespace Client
{
    internal class Program
    {
        private static Config _clusterConfig;

        public static void Main(string[] args)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            _clusterConfig = section.AkkaConfig;
            
            var port = args.Length > 0 ? args[0] : "0";
            var config =
                ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port=" + port)
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [frontend]"))
                    .WithFallback(_clusterConfig);

            var system = ActorSystem.Create("ClusterSystem", config);

            var frontend = system.ActorOf(Props.Create<Transformation>(), "frontend");
            var interval = TimeSpan.FromSeconds(2);
            var timeout = TimeSpan.FromSeconds(5);
            var counter = new AtomicCounter();

            system.Scheduler.Advanced.ScheduleRepeatedly(interval, interval,
                () => frontend.Ask(new TransformationMessages.TransformationJob("hello-" + counter.GetAndIncrement()),
                        timeout)
                    .ContinueWith(
                        r => Console.WriteLine(r.Result)));
            
            //starting 2 frontend nodes and 3 backend nodes
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}