using System;
using SqDotNet;

namespace Bloom
{
    public class SqObject
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

        public void PushSelf()
        {
            VM.PushObject(ObjectRef);
        }
    }
}
