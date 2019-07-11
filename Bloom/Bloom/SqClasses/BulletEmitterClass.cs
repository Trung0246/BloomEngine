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

        public MemberHandle HndPosition { get; }
        public MemberHandle HndAngle { get; }
        public MemberHandle HndFire { get; }
        public MemberHandle HndFireBullet { get; }

        private BulletEmitterClass() : base("BulletEmitter")
        {
            HndPosition =   NewField("Position");
            HndAngle =      NewField("Angle");

            SetConstructor(ScriptHandler.MakeFunction(Constructor));
            HndFire =       NewMethod("Fire", ScriptHandler.MakeFunction(Fire));
            HndFireBullet = NewMethod("FireBullet", ScriptHandler.MakeFunction(FireBullet));
        }

        public static void Register()
        {
            RegisteredClass = new BulletEmitterClass();
            ScriptHandler.SetGlobal("BulletEmitter", RegisteredClass);
        }

        public static int Constructor(Squirrel vm, int argCount)
        {
            var self = (SqInstance)ScriptHandler.This;
            self[RegisteredClass.HndPosition] = ScriptHandler.GetArg<Vector2>(0);
            self[RegisteredClass.HndAngle] = ScriptHandler.GetArg<float>(1);
            return 0;
        }

        public static int Fire(Squirrel vm, int argCount)
        {
            return 0;
        }

        public static int FireBullet(Squirrel vm, int argCount)
        {
            var pos = ScriptHandler.GetArg<Vector2>(0, false);
            var ang = ScriptHandler.GetArg<float>(1);
            var speed = ScriptHandler.GetArg<float>(2);
            var textureRegion = ScriptHandler.GetArg<TextureRegion>(3);
            GameScene.Current.BulletHandler.FireRaw(
                    new Vector3(pos, 0f),
                    ang,
                    speed,
                    textureRegion,
                    null
                );
            return 0;
        }
    }
}
