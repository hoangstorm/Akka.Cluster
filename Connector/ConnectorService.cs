using System;
using System.Threading;
using Akka.Actor;
using Shared.Actors;
using Topshelf;

namespace Connector
{
    public class ConnectorService : ServiceControl
    {
        private ActorSystem _actorSystem;
        private HostControl _hostControl;
        private static readonly ManualResetEvent asTerminatedEvent = new ManualResetEvent(false);

        public bool Start(HostControl hostControl)
        {
            _hostControl = hostControl;
            InitializeCluster();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Program.ClusterHelper.Tell(new ClusterHelper.RemoveMember());

            var cluster = Akka.Cluster.Cluster.Get(Program.ClusterSystem);
            cluster.RegisterOnMemberRemoved(() => MemberRemoved(Program.ClusterSystem));
            asTerminatedEvent.WaitOne();

            return true;
        }

        private async void MemberRemoved(ActorSystem actorSystem)
        {
            await actorSystem.Terminate();
            asTerminatedEvent.Set();
            Console.WriteLine("Member Removed");
        }

        public void InitializeCluster()
        {
            _actorSystem = ActorSystemFactory.LaunchClusterManager("Connector", "127.0.0.1", 2053);
            Program.ClusterSystem = _actorSystem;
            Program.ClusterHelper = Program.ClusterSystem.ActorOf(Props.Create(() => new ClusterHelper()), "connector");
        }
    }
}