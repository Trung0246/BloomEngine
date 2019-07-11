using System;
using System.Collections.Generic;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom
{
    public class SqUserData : SqObject
    {
        public IntPtr UserDataPointer
        {
            get
            {
                PushSelf();
                VM.GetUserData(-1, out var ptr, out _);
                VM.Pop(1);
                return ptr;
            }
        }

        public SqUserData(uint size) : this(GenerateUserDataRef(ScriptHandler.Squirrel, size))
        {
            VM.Pop(1);
        }

        public SqUserData(SqDotNet.Object userDataRef)
            : base(ScriptHandler.Squirrel, userDataRef)
        {
        }

        private static SqDotNet.Object GenerateUserDataRef(Squirrel vm, uint size)
        {
            vm.NewUserData(size);
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqObject);
        }

        public static bool operator ==(SqUserData left, SqUserData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SqUserData left, SqUserData right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
