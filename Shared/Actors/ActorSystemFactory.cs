using System;
using System.Configuration;
using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;

namespace Shared.Actors
{
    public static class ActorSystemFactory
    {
        public static ActorSystem LaunchClusterManager(string actorSystemName, string serverIp, int serverPort)
        {
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var akkaConfig = section.AkkaConfig;

            var injectTcpInfo = string.Format(@"akka.remote.helios.tcp.public-hostname = {0}{1} akka.remote.helios.tcp.hostname = {2}{3} akka.remote.helios.tcp.port = {4}{5}",
                serverIp,
                Environment.NewLine,
                serverIp,
                Environment.NewLine,
                serverPort,
                Environment.NewLine);

            var finalConfig = ConfigurationFactory.ParseString(
                injectTcpInfo)
                .WithFallback(akkaConfig);
            
            return ActorSystem.Create(actorSystemName, finalConfig);
        }

    }
}
