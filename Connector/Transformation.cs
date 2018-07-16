//-----------------------------------------------------------------------
// <copyright file="TransformationBackend.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Cluster;
using Shared;

namespace Connector
{
    public class Transformation : UntypedActor
    {
        protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);
        protected List<IActorRef> Backends = new List<IActorRef>();
        protected List<IActorRef> Clients = new List<IActorRef>();
        protected int Jobs = 0;

        /// <summary>
        /// Need to subscribe to cluster changes
        /// </summary>
        protected override void PreStart()
        {
            Cluster.Subscribe(Self, new[] {typeof(ClusterEvent.MemberUp)});
            Cluster.RegisterOnMemberUp(() =>
            {
                // create routers and other things that depend on me being UP in the cluster
            });
        }

        /// <summary>
        /// Re-subscribe on restart
        /// </summary>
        protected override void PostStop()
        {
            Cluster.Unsubscribe(Self);
        }

        protected override void OnReceive(object message)
        {
            Console.WriteLine(message);

            if (message is ClusterEvent.CurrentClusterState)
            {
                var state = (ClusterEvent.CurrentClusterState) message;
                foreach (var member in state.Members)
                {
                    if (member.Status == MemberStatus.Up)
                    {
                        Register(member);
                    }
                }
            }
            else if (message is ClusterEvent.MemberUp)
            {
                var memUp = (ClusterEvent.MemberUp) message;
                Register(memUp.Member);
            }

            if (message is TransformationMessages.TransformationJob && Backends.Count == 0)
            {
                var job = (TransformationMessages.TransformationJob) message;
                Sender.Tell(new TransformationMessages.JobFailed("Backend Service unavailable, try again later.", job),
                    Sender);
            }
            else if (message is TransformationMessages.TransformationJob)
            {
                Console.WriteLine(message);
                var job = (TransformationMessages.TransformationJob) message;
                Jobs++;

                var timeout = TimeSpan.FromSeconds(5);

                IActorRef sender = Sender;

                Console.WriteLine(Sender);

                Backends[Jobs % Backends.Count].Ask(message, timeout)
                    .ContinueWith(
                        r => { sender.Tell(r.Result); });
            }
            else if (message is TransformationMessages.TransformationResult)
            {
                Console.WriteLine("Result");
                Console.WriteLine(message);
                var job = (TransformationMessages.TransformationResult) message;
                Jobs++;
            }
            else if (message.Equals(TransformationMessages.BACKEND_REGISTRATION))
            {
                Context.Watch(Sender);
                Backends.Add(Sender);
            }
            else
            {
                Unhandled(message);
            }
        }

        protected void Register(Member member)
        {
            if (member.HasRole("frontend"))
            {
                Console.WriteLine("frontend callback");
                Console.WriteLine(Sender);
                Console.WriteLine("====frontend callback");

                Context.ActorSelection(member.Address + "/user/frontend")
                    .Tell(TransformationMessages.CONNECTOR_REGISTRATION, Self);
            }

            if (member.HasRole("backend"))
            {
                Console.WriteLine("backend callback");
                Console.WriteLine(Sender);
                Console.WriteLine("====backend callback");

                Context.ActorSelection(member.Address + "/user/backend")
                    .Tell(TransformationMessages.CONNECTOR_REGISTRATION, Self);
            }
        }
    }
}