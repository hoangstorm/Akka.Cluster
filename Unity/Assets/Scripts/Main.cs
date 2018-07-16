using System;
using System.Collections;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using Akka.Util.Internal;
using Shared;
using UnityEngine;

public class Main : MonoBehaviour
{
    private ActorSystem _system;
    private IActorRef _chatClient;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");

        var config = ConfigurationFactory.ParseString(@"
akka {
                    actor {
                      provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        }
                    
        remote {
            log-remote-lifecycle-events = DEBUG
            dot-netty.tcp {
                hostname = ""127.0.0.1""
                port = 0
            }
        }
                    
        cluster {
            seed-nodes = [
                ""akka.tcp://ClusterSystem@127.0.0.1:2551""]
                roles = [frontend]
                #auto-down-unreachable-after = 10s
        }
    }"
        );
        _system = ActorSystem.Create("ClusterSystem", config);
        var interval = TimeSpan.FromSeconds(2);
        var timeout = TimeSpan.FromSeconds(5);
        var counter = new AtomicCounter();

        _chatClient = _system.ActorOf(Props.Create<Transformation>(), "frontend");
        
        _system.Scheduler.Advanced.ScheduleRepeatedly(interval, interval,
            () => 
        _chatClient.Ask(new TransformationMessages.TransformationJob("hello-" + counter.GetAndIncrement()),
                timeout)
            .ContinueWith(
                r => Debug.Log(r.Result)));
    }

    // Update is called once per frame
    void Update()
    {
    }
}