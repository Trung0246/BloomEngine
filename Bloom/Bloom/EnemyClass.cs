using System;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom
{
    public class EnemyClass : SqClass
    {
        public static EnemyClass RegisteredClass { get; private set; }

        private EnemyClass()
        {
        }

        public static void Register()
        {
            RegisteredClass = new EnemyClass();
            ScriptHandler.SetGlobal("Enemy", RegisteredClass);
        }

        public static int Test(Squirrel vm, int argCount)
        {
            Console.Write(ScriptHandler.This["Foo"]);
            return 0;
        }
    }
}
