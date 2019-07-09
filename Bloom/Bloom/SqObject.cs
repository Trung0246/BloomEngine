using System;
using System.Collections.Generic;
using SqDotNet;
using Bloom.Handlers;

namespace Bloom
{
    public class SqObject : IEquatable<SqObject>
    {
        protected Squirrel VM { get; }
        public SqDotNet.Object ObjectRef { get; }

        public SqObject(Squirrel vm, SqDotNet.Object objRef)
        {
            VM = vm;
            ObjectRef = objRef;
            VM.AddRef(ObjectRef);
        }

        ~SqObject()
        {
            VM.Release(ObjectRef);
        }

        public virtual void PushSelf()
        {
            VM.PushObject(ObjectRef);
        }

        public ObjectType GetObjectType()
        {
            PushSelf();
            var type = VM.GetTypeFixed(-1);
            VM.Pop(1);
            return type;
        }

        public void SetDelegate(SqTable table)
        {
            PushSelf();
            VM.PushTable(table);
            VM.SetDelegate(VM.GetTop() - 1);
            VM.Pop(1);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqObject);
        }

        public bool Equals(SqObject other)
        {
            return other != null &&
                   ObjectRef.Equals(other.ObjectRef);
        }

        public override int GetHashCode()
        {
            return ObjectRef.GetHashCode();
        }

        public static bool operator ==(SqObject left, SqObject right)
        {
            return EqualityComparer<SqObject>.Default.Equals(left, right);
        }

        public static bool operator !=(SqObject left, SqObject right)
        {
            return !(left == right);
        }
    }
}
