//-----------------------------------------------------------------------
// <copyright file="TransformationFrontend.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2018 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2018 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Shared;
using UnityEngine;
using Debug = Akka.Event.Debug;

public class Transformation : UntypedActor
{
    protected List<IActorRef> Connectors = new List<IActorRef>();
    protected int Jobs = 0;

    protected override void OnReceive(object message)
    {
        if (message is TransformationMessages.TransformationJob && Connectors.Count == 0)
        {
            var job = (TransformationMessages.TransformationJob) message;
            Sender.Tell(new TransformationMessages.JobFailed("Service unavailable, try again later.", job), Sender);
        }
        else if (message is TransformationMessages.TransformationJob)
        {
            var job = (TransformationMessages.TransformationJob) message;
            Jobs++;
            Connectors[Jobs % Connectors.Count].Forward(job);
        }
        else if (message.Equals(TransformationMessages.CONNECTOR_REGISTRATION))
        {
            UnityEngine.Debug.Log("register");
            UnityEngine.Debug.Log(Sender);
            Connectors.Add(Sender);
        }
        else if (message is Terminated)
        {
            var terminated = (Terminated) message;
            Connectors.Remove(terminated.ActorRef);
        }
        else
        {
            Console.WriteLine("====VCL=====");
            Console.WriteLine(message);
            Unhandled(message);
        }
    }
}