using System;
using System.Collections.Generic;
using Bloom.Handlers;
using Bloom.Scenes;
using SqDotNet;

namespace Bloom.SqClasses
{
    public class TimerClass : SqClass
    {
        public static TimerClass RegisteredClass { get; private set; }

        private static Dictionary<SqInstance, TimerHandler.Timer> Timers { get; }
            = new Dictionary<SqInstance, TimerHandler.Timer>();

        private TimerClass() : base("Timer")
        {
            SetConstructor(ScriptHandler.MakeFunction(Constructor));

            NewMethod("Stop", ScriptHandler.MakeFunction(Stop));
        }

        public static void Register()
        {
            RegisteredClass = new TimerClass();
            ScriptHandler.SetGlobal("Timer", RegisteredClass);
        }

        public static int Constructor(Squirrel vm, int argCount)
        {
            var self = ((SqInstance)ScriptHandler.This);
            var duration = ScriptHandler.GetArg<double>(0);
            var times = ScriptHandler.GetArg<int>(1);
            var func = ScriptHandler.GetArg<SqClosure>(2);
            Timers.Add(self, GameScene.Current.TimerHandler.StartTimer(
                    duration, (_) =>
                    {
                        func.CallAsMethod(self);
                        if (Timers[self].Times == 1)
                            Timers.Remove(self);
                    }
                ));
            return 0;
        }

        public static int Stop(Squirrel vm, int argCount)
        {
            var self = ((SqInstance)ScriptHandler.This);
            if (!Timers.TryGetValue(self, out var timer))
                throw new InvalidOperationException("Cannot stop a Timer that is not running");
            Timers.Remove(self);
            timer.Stop();
            return 0;
        }
    }
}
