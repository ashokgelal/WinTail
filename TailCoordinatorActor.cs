﻿using System;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }

            public IActorRef ReporterActor { get; private set; }
        }

        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; private set; }
        }

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // max retries
                TimeSpan.FromSeconds(30),  // within time range
                x => // localOnlyDecider
                {
                    if (x is ArithmeticException)
                        return Directive.Resume;
                    else if (x is NotSupportedException)
                        return Directive.Stop;
                    else
                        return Directive.Restart;
                });
        }
    }
}

