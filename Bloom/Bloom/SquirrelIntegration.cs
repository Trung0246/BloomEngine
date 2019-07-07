using System;
using System.Reflection;
using System.Collections.Generic;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom
{
#if DUMMY
    public static class SquirrelIntegration
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public class SquirrelClassAttribute : Attribute
        {
            public string Name;

            public SquirrelClassAttribute(string name)
            {
                Name = name;
            }
        }

        [AttributeUsage(
                AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
                AllowMultiple = false,
                Inherited = true
            )]
        public class SquirrelPropertyAttribute : Attribute
        {
            public string Name;

            public SquirrelPropertyAttribute(string name)
            {
                Name = name;
            }
        }

        private static Dictionary<Type, string> RegisteredClasses { get; } = new Dictionary<Type, string>();
        private static Dictionary<Type, Dictionary<string, (Action<object>, Func<object>)>> PropertyFuncDicts { get; }
            = new Dictionary<Type, Dictionary<string, (Action<object>, Func<object>)>>();
        private static Squirrel VM => ScriptHandler.Squirrel;

        public static bool ClassRegistered(Type type)
        {
            return RegisteredClasses.ContainsKey(type);
        }

        public static string GetClassName(Type type)
        {
            return RegisteredClasses[type];
        }

        public static void RegisterClasses(Assembly assembly)
        {
            foreach (var asmClass in assembly.GetTypes())
            {
                var attr = asmClass.GetCustomAttribute<SquirrelClassAttribute>();
                if (attr is null)
                    continue;
                RegisterClass(asmClass, attr.Name);
            }
        }

        public static void RegisterClass(Type type, string name)
        {
            if (ClassRegistered(type))
                return;
            var baseType = type.GetTypeInfo().BaseType;
            var baseAttr = baseType.GetCustomAttribute<SquirrelClassAttribute>();
            if (baseAttr is null)
            {
                Console.WriteLine($"Registering class \"{type}\" as Squirrel class \"{name}\"");
                VM.NewClass(false);
            }
            else
            {
                RegisterClass(baseType, baseAttr.Name);
                ScriptHandler.PushGlobal(baseAttr.Name);
                Console.WriteLine($"Registering class \"{type}\" as Squirrel class \"{name}\"");
                VM.NewClass(true);
            }
            var propFuncs = new Dictionary<string, (Action<object>, Func<object>)>();
            PropertyFuncDicts.Add(type, propFuncs);
            foreach (var member in type.GetFields())
            {
                var attr = member.GetCustomAttribute<SquirrelClassAttribute>();
                if (attr is null)
                    continue;
                Console.WriteLine($"\tRegistering field \"{member.Name}\" as Squirrel member \"{name}.{attr.Name}\"");
                /*
                // Name
                VM.PushString(attr.Name, -1);
                // Value
                try
                {
                    var startingValue = member.GetRawConstantValue();
                    VM.PushDynamic(startingValue);
                    Console.WriteLine($"\t\tValue = {startingValue}");
                }
                catch (Exception _)
                {
                    VM.PushNull();
                    Console.WriteLine($"\t\tValue = null");
                }
                // Attribute
                VM.PushNull();
                VM.NewMember(-4, member.IsStatic);*/
                propFuncs.Add(
                        attr.Name,
                        (
                            (value) =>
                            {

                            },
                            () =>
                            {
                                
                            }
                        )
                    );
            }
            ScriptHandler.PopToGlobal(name);
            RegisteredClasses.Add(type, name);
        }
    }
#endif
}
