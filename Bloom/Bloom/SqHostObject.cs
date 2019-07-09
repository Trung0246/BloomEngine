using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom
{
    public class SqHostObject : SqUserData
    {
        public class ReferencedObject
        {
            private static long NextHandleIndex;

            public long Index;
            public GCHandle Handle;
            public Squirrel.Unmanaged.SqReleaseHook ReleaseHook;

            public ReferencedObject(SqHostObject hostObj, object obj, out long handleIndex)
            {
                try
                {
                    Handle = GCHandle.Alloc(obj, GCHandleType.Normal);
                }
                catch (Exception e)
                {
                    throw new Exception($"Could not allocate object of type {obj.GetType()} when making an SqHostObject: {e.ToString()}");
                }
                ReleaseHook = new Squirrel.Unmanaged.SqReleaseHook(
                        (ptr, _) =>
                        {
                            unsafe
                            {
                                Handles[*(long*)ptr].Handle.Free();
                                Handles.Remove(*(long*)ptr);
                            }
                            return 1;
                        }
                    );
                Index = NextHandleIndex;
                var attemptsToFindNextIndex = 0UL;
                do
                {
                    NextHandleIndex++;
                    attemptsToFindNextIndex++;
                } while (attemptsToFindNextIndex < ulong.MaxValue && Handles.ContainsKey(NextHandleIndex));
                handleIndex = Index;
                Handles.Add(handleIndex, this);

                hostObj.PushSelf();
                ScriptHandler.Squirrel.SetReleaseHook(-1, ReleaseHook);
                ScriptHandler.Squirrel.Pop(1);
            }
        }

        public static Dictionary<long, ReferencedObject> Handles { get; }
            = new Dictionary<long, ReferencedObject>();

        private long HandleIndex
        {
            get
            {
                unsafe
                {
                    return *(long*)UserDataPointer;
                }
            }
        }

        public object Object => Handles[HandleIndex].Handle.Target;

        public SqHostObject(object obj)
            : base((uint)IntPtr.Size)
        {
            var refObj = new ReferencedObject(this, obj, out var handleIndex);
            unsafe
            {
                *(long*)UserDataPointer = handleIndex;
            }
        }

        public SqHostObject(SqDotNet.Object pointerRef)
            : base(pointerRef)
        {
        }

        public override bool Equals(object obj)
        {
            return Object.Equals(obj);
        }

        public static bool operator ==(SqHostObject left, SqHostObject right)
        {
            return left.Object.Equals(right.Object);
        }

        public static bool operator !=(SqHostObject left, SqHostObject right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }
    }
}
