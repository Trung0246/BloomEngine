using SqDotNet;
using Bloom.Handlers;
using System;

namespace Bloom
{
    public class SqClosure : SqObject
    {
        public SqClosure(Func<Squirrel, int, int> func) : this(GenerateClosureRef(ScriptHandler.Squirrel, func))
        {
            VM.Pop(1);
        }

        public SqClosure(SqDotNet.Object instanceRef)
            : base(ScriptHandler.Squirrel, instanceRef)
        {
        }

        private static SqDotNet.Object GenerateClosureRef(Squirrel vm, Func<Squirrel, int, int> func)
        {
            vm.NewClosure(ScriptHandler.MakeFunction(func), 0);
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public object[] CallAsClosure(params object[] arguments)
        {
            PushSelf();
            VM.PushRootTable();
            return ScriptHandler.PopToCallAsMethod(-2, arguments);
        }

        public object[] CallAsMethod(SqObject receiver, params object[] arguments)
        {
            PushSelf();
            receiver.PushSelf();
            return ScriptHandler.PopToCallAsMethod(-2, arguments);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqObject);
        }

        public static bool operator ==(SqClosure left, SqClosure right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SqClosure left, SqClosure right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
