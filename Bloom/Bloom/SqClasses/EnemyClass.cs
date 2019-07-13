using System.Numerics;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom.SqClasses
{
#if DUMMY
    public class ActorClass : SqClass
    {
        public static ActorClass RegisteredClass { get; private set; }

        // Fields
        public MemberHandle HndInternalActor { get; }
        // Methods
        public MemberHandle HndConstructor { get; }
        public MemberHandle HndSetPosition { get; }
        public MemberHandle HndSetHealth { get; }

        private ActorClass() : base("Actor")
        {
            HndInternalActor = NewField("_InternalActor");

            HndConstructor = SetConstructor(ScriptHandler.MakeFunction(Constructor));
            HndSetPosition = NewMethod("SetPosition", ScriptHandler.MakeFunction(SetPosition));
            HndSetPosition = NewMethod("SetHealth", ScriptHandler.MakeFunction(SetHealth));
        }

        public static void Register()
        {
            RegisteredClass = new ActorClass();
            ScriptHandler.SetGlobal("Actor", RegisteredClass);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <sqparam type="Vector3" name="position" optional=false>The Actor's position</sqparam>
        /// <sqreturns></sqreturns>
        private static int Constructor(Squirrel vm, int argCount)
        {
            var self = (SqInstance)ScriptHandler.This;
            var argPosition = ScriptHandler.GetArg<Vector3>(0);
            self.CallMember(RegisteredClass.HndSetPosition, argPosition);
            self.CallMember(RegisteredClass.SetHealth, 100);
            return 0;
        }

        /// <summary>
        /// Set the actor's position
        /// </summary>
        /// <sqparam type="Vector3" name="position" optional=false>The Actor's position</sqparam>
        /// <sqreturns></sqreturns>
        private static int SetPosition(Squirrel vm, int argCount)
        {
            var self = (SqInstance)ScriptHandler.This;
            var argPosition = ScriptHandler.GetArg<Vector3>(0);
            var internalActor = self[RegisteredClass.HndInternalActor];
            return 0;
        }
    }
#endif
}
