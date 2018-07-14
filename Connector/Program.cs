using System;
using Akka.Actor;
using Topshelf;

namespace Connector
{
    internal class Program
    {
        public static ActorSystem ClusterSystem { get; set; }
        public static IActorRef ClusterHelper;
        public static IActorRef ClusterStatus;
        

        public static void Main(string[] args)
        {
            return (int) HostFactory.Run(x =>
            {
                x.SetServiceName("Connector");
                x.SetDisplayName("Connector Service Discovery");
                x.SetDescription("Connector Service Discovery for Akka.NET Clusters");

                x.UseAssemblyInfoForServiceInfo();
                x.RunAsLocalSystem();
                //x.StartAutomatically();
                x.DependsOnEventLog();
                x.UseLog4Net();
                x.Service<ConnectorService>();
                x.EnableServiceRecovery(r => r.RestartService(1));
            });
        }
    }
}