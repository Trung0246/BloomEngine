using System;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom
{
    public class SqClass : SqObject
    {
        public object this[string key]
        {
            get
            {
                PushSelf();
                VM.PushString(key, -1);
                if (!VM.GetFixed(-2).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to get member \"{key}\"");
                }
                var ret = VM.GetDynamic(-1);
                VM.Pop(2);
                return ret;
            }
        }

        public SqClass(SqClass baseClass = null) : this(GenerateClassRef(ScriptHandler.Squirrel, baseClass))
        {
            VM.Pop(baseClass is null ? 1 : 2);
        }

        public SqClass(SqDotNet.Object classRef)
            : base(ScriptHandler.Squirrel, classRef)
        {
        }

        private static SqDotNet.Object GenerateClassRef(Squirrel vm, SqClass baseClass)
        {
            var hasBase = !(baseClass is null);
            if (hasBase)
                baseClass.PushSelf();
            vm.NewClass(hasBase);
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public void PushMember(string key)
        {
            PushSelf();
            VM.PushString(key, -1);
            if (!VM.GetFixed(-2).IsOK())
            {
                VM.Pop(1);
                throw new Exception($"Unable to get member \"{key}\"");
            }
            VM.RemoveFixed(-2);
        }

        public void CallMember(string key, params object[] arguments)
        {
            PushMember(key);
            PushSelf();
            ScriptHandler.PopToCallMethod(-2, arguments);
        }

        public void NewMember(string key, object value, bool isStatic = false)
        {
            if (ContainsMember(key))
                throw new InvalidOperationException($"Class already contains the member {key}");
            PushSelf();
            VM.PushString(key, -1);
            VM.PushDynamic(value);
            VM.PushNull();
            if (!VM.NewMember(-4, isStatic).IsOK())
            {
                VM.Pop(1);
                throw new Exception($"Unable to create/set member \"{key}\"");
            }
            VM.Pop(1);
        }

        public void NewMethod(string key, Function func, bool isStatic = false)
        {
            NewMember(key, func, isStatic);
        }

        public void NewField(string key, object value, bool isStatic = false)
        {
            NewMember(key, value, isStatic);
        }

        public bool ContainsMember(string key)
        {
            PushSelf();
            VM.PushString(key, -1);
            var found = VM.GetFixed(-2).IsOK();
            if (found)
            {
                VM.Pop(2);
                return true;
            }
            VM.Pop(1);
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            PushSelf();
            VM.PushString(key, -1);
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

        public bool Remove(string key)
        {
            PushSelf();
            VM.PushString(key, -1);
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

        public static bool operator ==(SqClass left, SqClass right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SqClass left, SqClass right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
