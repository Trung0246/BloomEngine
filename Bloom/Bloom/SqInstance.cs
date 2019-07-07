using SqDotNet;
using Bloom.Handlers;
using System;

namespace Bloom
{
    public class SqInstance : SqObject
    {
        public object this[object key]
        {
            get
            {
                PushSelf();
                VM.PushDynamic(key);
                if (!VM.GetFixed(-2).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to get member/slot with key {key}");
                }
                var ret = VM.GetDynamic(-1);
                VM.Pop(2);
                return ret;
            }
            set
            {
                PushSelf();
                VM.PushDynamic(key);
                VM.PushDynamic(value);
                if (!VM.NewSlot(-3, false).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to create/set slot with key {key}");
                }
                VM.Pop(1);
            }
        }

        public SqInstance(SqClass sqClass) : this(GenerateInstanceRef(ScriptHandler.Squirrel, sqClass))
        {
            VM.Pop(2);
        }

        public SqInstance(SqDotNet.Object instanceRef)
            : base(ScriptHandler.Squirrel, instanceRef)
        {
        }

        private static SqDotNet.Object GenerateInstanceRef(Squirrel vm, SqClass sqClass)
        {
            sqClass.PushSelf();
            vm.CreateInstance(-1);
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public void PushValue(object key)
        {
            PushSelf();
            VM.PushDynamic(key);
            if (!VM.GetFixed(-2).IsOK())
            {
                VM.Pop(1);
                throw new Exception($"Unable to read slot {key}");
            }
            VM.RemoveFixed(-2);
        }

        public void CallMemberOrSlot(object key, params object[] arguments)
        {
            PushValue(key);
            PushSelf();
            ScriptHandler.PopToCallMethod(-2, arguments);
        }

        public bool ContainsMemberOrSlot(object key)
        {
            PushSelf();
            VM.PushDynamic(key);
            var found = VM.GetFixed(-2).IsOK();
            if (found)
            {
                VM.Pop(2);
                return true;
            }
            VM.Pop(1);
            return false;
        }

        public bool TryGetValue(object key, out object value)
        {
            PushSelf();
            VM.PushDynamic(-1);
            var found = VM.GetFixed(-2).IsOK();
            if (found)
            {
                value = VM.GetDynamic(-1);
                VM.Pop(2);
                return true;
            }
            VM.Pop(1);
            value = default;
            return false;
        }

        public bool RemoveMemberOrSlot(object key)
        {
            PushSelf();
            VM.PushDynamic(-1);
            var deleted = VM.DeleteSlot(-2, false).IsOK();
            VM.Pop(1);
            return deleted;
        }

        public void Clear()
        {
            PushSelf();
            VM.Clear(-1);
            VM.Pop(1);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqObject);
        }

        public static bool operator ==(SqInstance left, SqInstance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SqInstance left, SqInstance right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
