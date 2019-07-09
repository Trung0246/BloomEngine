using System;
using System.Numerics;
using Bloom.Handlers;
using Bloom.Scenes;
using SqDotNet;
using WyvernFramework;

namespace Bloom.SqClasses
{
    public class BulletEmitterClass : SqClass
    {
        public static BulletEmitterClass RegisteredClass { get; private set; }

        private BulletEmitterClass() : base("BulletEmitter")
        {
            NewField("Position", VectorClass.RegisteredClass.CallConstructor(0f, 0f, 0f));
            NewField("Angle", 0f);

            SetConstructor(ScriptHandler.MakeFunction(Constructor));
            NewMethod("Fire", ScriptHandler.MakeFunction(Fire));
            NewMethod("FireBullet", ScriptHandler.MakeFunction(FireBullet));
        }

        public static void Register()
        {
            RegisteredClass = new BulletEmitterClass();
            ScriptHandler.SetGlobal("BulletEmitter", RegisteredClass);
        }

        public static int Constructor(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            self["Position"] = ScriptHandler.GetArg(0, VectorClass.RegisteredClass, "Vector");
            self["Angle"] = ScriptHandler.GetArg<float>(1);
            return 0;
        }

        public static int Fire(Squirrel vm, int argCount)
        {
            return 0;
        }

        public static int FireBullet(Squirrel vm, int argCount)
        {
            var pos = ScriptHandler.GetArg(0, VectorClass.RegisteredClass, "Vector");
            var ang = ScriptHandler.GetArg<float>(1);
            var speed = ScriptHandler.GetArg<float>(2);
            var textureRegion = ScriptHandler.GetArg<TextureRegion>(3);
            GameScene.Current.BulletHandler.FireRaw(
                    new Vector3(pos.Get<float>("X"), pos.Get<float>("Y"), pos.Get<float>("Z")),
                    ang,
                    speed,
                    textureRegion,
                    null
                );
            return 0;
        }
    }
}
