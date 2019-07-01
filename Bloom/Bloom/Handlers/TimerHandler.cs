﻿using WyvernFramework;
using Bloom.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bloom.Handlers
{
    public class TimerHandler : IDebug
    {
        public class Timer
        {
            public TimerHandler Handler { get; }
            public double Duration { get; private set; }
            public double EndTime { get; private set; }
            public Action Action { get; }
            public int Times { get; private set; }
            public bool Infinite => Times == -1;
            public bool Ready => Handler.Scene.Graphics.CurrentTime >= EndTime;
            public bool Ended { get; private set; }

            public Timer(TimerHandler handler, double duration, Action action, int times)
            {
                Handler = handler;
                Duration = duration;
                Action = action;
                EndTime = Handler.Scene.Graphics.CurrentTime + duration;
                Times = times;
            }

            public void Execute()
            {
                Action();
                if (Times > -1)
                    Times--;
                if (Times == 0)
                {
                    Ended = true;
                }
                else
                {
                    EndTime += Duration;
                }
            }
        }

        public string Name => nameof(BulletHandler);

        public string Description => "The rimwe handler";

        private GameScene Scene { get; }

        public List<Timer> Timers { get; } = new List<Timer>();

        public TimerHandler(GameScene scene)
        {
            Scene = scene;
        }

        public void End()
        {
        }

        public void Update()
        {
            foreach (
                    var timer in Timers
                        .Where(e => e.Ready)
                        .ToArray()
                )
            {
                while (timer.Ready)
                {
                    timer.Execute();
                    if (timer.Ended)
                        Timers.Remove(timer);
                }
            }
        }

        public void StartTimer(double duration, Action action, int times = -1)
        {
            if (duration < 0.0)
                throw new ArgumentOutOfRangeException(nameof(duration));
            if (times < -1 || times == 0)
                throw new ArgumentOutOfRangeException(nameof(times));
            Timers.Add(new Timer(this, duration, action, times));
        }
    }
}