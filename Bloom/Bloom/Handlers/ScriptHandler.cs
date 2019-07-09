using SqDotNet;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using WyvernFramework;
using Bloom.SqClasses;

namespace Bloom.Handlers
{
    public static class ScriptHandler
    {
        public static string ScriptRoot => Path.Combine(ContentCollection.ContentRoot, "Scripts");

        public static Squirrel Squirrel { get; private set; }

        public static void Init()
        {
            Squirrel = new Squirrel();
            Squirrel.SetPrintFunc(OnPrint, OnError);
            Squirrel.PushRootTable();
            Squirrel.RegisterBlobLib();
            Squirrel.RegisterIOLib();
            Squirrel.RegisterSystemLib();
            Squirrel.RegisterMathLib();
            Squirrel.RegisterStringLib();
            Squirrel.SetErrorHandlers();
            //Squirrel.EnableDebugInfo(true);

            RegisterCores();
            VectorClass.Register();
            TimerClass.Register();
            BulletEmitterClass.Register();
            EnemyClass.Register();

            PushCompiledFile("classes.nut");
            Squirrel.PushRootTable();
            PopToCallAsMethod(-2);
            //CallGlobal("main");

            var hobj = new SqHostObject(5);
            hobj.PushSelf();
            hobj = Squirrel.GetHostObject(-1);
            Squirrel.Pop(1);
            Console.WriteLine(hobj.Object);
        }

        public static void Close()
        {
            Squirrel.Dispose();
        }

        private static void OnPrint(Squirrel v, string message)
        {
            Console.WriteLine(message);
        }

        private static void OnError(Squirrel v, string message)
        {
            Debug.Write(ConsoleColor.Black, ConsoleColor.Red, message);
        }

        public static bool IsOK(this int result)
        {
            return result == 0;
        }

        /// <summary>
        /// Push a slot from the current root table
        /// </summary>
        /// <param name="key"></param>
        public static void PushGlobal(string key)
        {
            Squirrel.PushRootTable();
            Squirrel.PushString(key, -1);
            if (!Squirrel.GetFixed(-2).IsOK())
                throw new Exception($"Unable to read slot with key \"{key}\"");
            Squirrel.RemoveFixed(-2);
        }

        /// <summary>
        /// Get a slot from the current root table
        /// </summary>
        /// <param name="key"></param>
        public static object GetGlobal(string key)
        {
            Squirrel.PushRootTable();
            Squirrel.PushString(key, -1);
            if (!Squirrel.GetFixed(-2).IsOK())
                throw new Exception($"Unable to read slot with key \"{key}\"");
            var obj = Squirrel.GetDynamic(-1);
            Squirrel.Pop(2);
            return obj;
        }

        /// <summary>
        /// Pop a value and set a slot in the root table to that value
        /// </summary>
        /// <param name="key"></param>
        public static void PopToGlobal(string key)
        {
            Squirrel.PushRootTable();
            Squirrel.PushString(key, -1);
            Squirrel.PushFixed(-3);
            if (!Squirrel.CreateSlot(-3).IsOK())
                throw new Exception($"Unable to create slot with key \"{key}\"");
            Squirrel.Pop(2);
        }

        /// <summary>
        /// Set a slot in the root table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void SetGlobal(string key, Function obj)
        {
            Squirrel.PushRootTable();
            Squirrel.PushString(key, -1);
            Squirrel.NewClosure(obj, 0);
            if (!Squirrel.NewSlot(-3, false).IsOK())
                throw new Exception($"Unable to create slot with key \"{key}\"");
            Squirrel.Pop(1);
        }

        /// <summary>
        /// Set a slot in the root table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void SetGlobal(string key, object obj)
        {
            Squirrel.PushRootTable();
            Squirrel.PushString(key, -1);
            Squirrel.PushDynamic(obj);
            if (!Squirrel.CreateSlot(-3).IsOK())
                throw new Exception($"Unable to create slot with key \"{key}\"");
            Squirrel.Pop(1);
        }

        /// <summary>
        /// Compile a source then push the function
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        public static void PushCompiledString(string source, string name)
        {
            if (!Squirrel.CompileBuffer(source, source.Length, name, true).IsOK())
                throw new Exception($"Unable to compile script \"{name}\"");
        }

        /// <summary>
        /// Compile a source then push the function
        /// </summary>
        /// <param name="path"></param>
        public static void PushCompiledFile(string path)
        {
            PushCompiledString(File.ReadAllText(Path.Combine(ScriptRoot, path)), path);
        }

        /// <summary>
        /// Pop function then call it
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object[] PopToCallAsMethod(int idx, params object[] arguments)
        {
            var top = Squirrel.GetTop() + idx + 1;
            foreach (var obj in arguments)
                Squirrel.PushDynamic(obj);
            if (!Squirrel.Call(Squirrel.GetTop() - top, true, true).IsOK())
            {
                Squirrel.Pop(1);
                throw new InvalidOperationException($"Unable to call the closure");
            }
            var returnCount = Squirrel.GetTop() - top;
            var returns = new object[returnCount];
            idx = top + 1;
            for (var i = 0; i < returnCount; i++)
                returns[i] = Squirrel.GetDynamic(idx++);
            Squirrel.Pop(1 + returnCount);
            return returns;
        }

        /// <summary>
        /// Call a function in a global slot
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object[] CallGlobal(string key, params object[] parameters)
        {
            PushGlobal(key);
            Squirrel.PushRootTable();
            return PopToCallAsMethod(-2, parameters);
        }

        /// <summary>
        /// Call a function in a global slot
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object[] CallGlobalAsMethod(SqObject receiver, string key, params object[] parameters)
        {
            PushGlobal(key);
            receiver.PushSelf();
            return PopToCallAsMethod(-2, parameters);
        }

        // Keep references to lambda functions to be used in squirrel
        private static Dictionary<Func<Squirrel, int, int>, Func<Squirrel, int>> LambdaFunctions
            = new Dictionary<Func<Squirrel, int, int>, Func<Squirrel, int>>();
        // Keep references to squirrel functions
        private static Dictionary<Func<Squirrel, int, int>, Function> SquirrelFunctions
            = new Dictionary<Func<Squirrel, int, int>, Function>();

        /// <summary>
        /// Make a Squirrel function
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Function MakeFunction(Func<Squirrel, int, int> func)
        {
            if (SquirrelFunctions.TryGetValue(func, out var cachedFunc))
                return cachedFunc;
            Func<Squirrel, int> lambda = (vm) => func(vm, vm.GetTop() - 1);
            var sqFunc = new Function(lambda);
            LambdaFunctions.Add(func, lambda);
            SquirrelFunctions.Add(func, sqFunc);
            return sqFunc;
        }

        /// <summary>
        /// Get an argument (if in a called function)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static object GetArg(int idx)
        {
            if (2 + idx > Squirrel.GetTop())
                return default;
            return Squirrel.GetDynamic(2 + idx);
        }

        /// <summary>
        /// Get an argument (if in a called function) and check the type (for Squirrel class types)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static SqInstance GetArg(int idx, SqClass sqClass, string className)
        {
            if (2 + idx > Squirrel.GetTop())
                return default;
            var val = Squirrel.GetDynamic(2 + idx);
            if (val is SqInstance instance && instance.IsInstanceOf(sqClass))
                return instance;
            throw ErrorHelper.WrongArgumentType(idx, val.GetType().Name, className);
        }

        /// <summary>
        /// Get an argument (if in a called function) and check the type
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static T GetArg<T>(int idx)
        {
            if (2 + idx > Squirrel.GetTop())
                return default;
            var val = Squirrel.GetDynamic(2 + idx);
            if (val is T already)
                return already;
            if (val is SqHostObject hostObj)
            {
                try
                {
                    return (T)Convert.ChangeType(hostObj.Object, typeof(T));
                }
                catch
                {
                    throw ErrorHelper.WrongArgumentType(idx, hostObj.Object.GetType().Name, typeof(T).Name);
                }
            }
            try
            {
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                if (val is SqInstance inst)
                {
                    throw ErrorHelper.WrongArgumentType(idx, $"{nameof(SqInstance)} (instanceof {inst.Class.GetType().Name})", typeof(T).Name);
                }
                throw ErrorHelper.WrongArgumentType(idx, val.GetType().Name, typeof(T).Name);
            }
        }

        /// <summary>
        /// The instance representing "this" during a call
        /// </summary>
        public static SqInstance This => Squirrel.GetInstance(1);

        // = Squirrel extensions =

        /// <summary>
        /// For whatever reason, the normal Squirrel.Get is broken with negative indices
        /// so this is a workaround extension method
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static int GetFixed(this Squirrel vm, int idx)
        {
            if (idx < 0)
                return vm.Get(Squirrel.GetTop() + idx + 1);
            return vm.Get(idx);
        }

        /// <summary>
        /// For whatever reason, the normal Squirrel.Set is broken with negative indices
        /// so this is a workaround extension method
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static int SetFixed(this Squirrel vm, int idx)
        {
            if (idx < 0)
                return vm.Set(Squirrel.GetTop() + idx + 1);
            return vm.Set(idx);
        }

        /// <summary>
        /// For whatever reason, the normal Squirrel.Push is broken with negative indices
        /// so this is a workaround extension method
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static void PushFixed(this Squirrel vm, int idx)
        {
            if (idx < 0)
                vm.Push(Squirrel.GetTop() + idx + 1);
            else
                vm.Push(idx);
        }

        /// <summary>
        /// For whatever reason, the normal Squirrel.Remove is broken with negative indices
        /// so this is a workaround extension method
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static void RemoveFixed(this Squirrel vm, int idx)
        {
            if (idx < 0)
                vm.Remove(Squirrel.GetTop() + idx + 1);
            else
                vm.Remove(idx);
        }

        /// <summary>
        /// For whatever reason, the normal Squirrel.GetType is broken with negative indices
        /// so this is a workaround extension method
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static ObjectType GetTypeFixed(this Squirrel vm, int idx)
        {
            if (idx < 0)
                return vm.GetType(Squirrel.GetTop() + idx + 1);
            return vm.GetType(idx);
        }

        /// <summary>
        /// Get the table at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqTable GetTable(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var tableRef);
            return new SqTable(tableRef);
        }

        /// <summary>
        /// Get the class at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqClass GetSqClass(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var classRef);
            return new SqClass(classRef);
        }

        /// <summary>
        /// Get the instance at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqInstance GetInstance(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var classRef);
            return new SqInstance(classRef);
        }

        /// <summary>
        /// Get the closure at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqClosure GetClosure(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var funcRef);
            return new SqClosure(funcRef);
        }

        /// <summary>
        /// Get the user data at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqUserData GetSqUserData(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var udRef);
            return new SqUserData(udRef);
        }

        /// <summary>
        /// Get the host object at idx in the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="idx"></param>
        public static SqHostObject GetHostObject(this Squirrel vm, int idx)
        {
            vm.GetStackObj(idx, out var objRef);
            return new SqHostObject(objRef);
        }

        /// <summary>
        /// Push a table on the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="table"></param>
        public static void PushTable(this Squirrel vm, SqTable table)
        {
            vm.PushObject(table.ObjectRef);
        }

        /// <summary>
        /// Push a class on the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="sqClass"></param>
        public static void PushClass(this Squirrel vm, SqClass sqClass)
        {
            vm.PushObject(sqClass.ObjectRef);
        }

        /// <summary>
        /// Push a host object on the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="obj"></param>
        public static void PushHostObject(this Squirrel vm, SqHostObject obj)
        {
            obj.PushSelf();
        }

        /// <summary>
        /// Push a host object on the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="obj"></param>
        public static void PushHostObject(this Squirrel vm, object obj)
        {
            new SqHostObject(obj).PushSelf();
        }

        /// <summary>
        /// Push an object to the stack with dynamic type checking
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="obj"></param>
        public static void PushDynamic(this Squirrel vm, object obj)
        {
            if (obj is null)
            {
                vm.PushNull();
                return;
            }
            switch (obj.GetType().Name)
            {
                default:
                    if (obj is SqObject sqObj)
                    {
                        vm.PushObject(sqObj.ObjectRef);
                        break;
                    }
                    new SqHostObject(obj).PushSelf();
                    throw new InvalidOperationException($"Cannot push value of type {obj.GetType().Name}");
                case nameof(Byte):
                    vm.PushInteger((byte)obj);
                    break;
                case nameof(Int16):
                    vm.PushInteger((short)obj);
                    break;
                case nameof(Int32):
                    vm.PushInteger((int)obj);
                    break;
                case nameof(Int64):
                    vm.PushInteger((int)(long)obj);
                    break;
                case nameof(IntPtr):
                    vm.PushUserPointer((IntPtr)obj);
                    break;
                case nameof(Single):
                    vm.PushFloat((float)obj);
                    break;
                case nameof(Double):
                    vm.PushFloat((float)(double)obj);
                    break;
                case nameof(Boolean):
                    vm.PushBool((bool)obj);
                    break;
                case nameof(String):
                    vm.PushString((string)obj, -1);
                    break;
                case nameof(Function):
                    Squirrel.NewClosure((Function)obj, 0);
                    break;
            }
        }

        /// <summary>
        /// Get the value at a position in the stack with dynamic type checking
        /// </summary>
        /// <returns></returns>
        public static object GetDynamic(this Squirrel vm, int idx)
        {
            var type = vm.GetTypeFixed(idx);
            switch (type)
            {
                default:
                    throw new InvalidOperationException($"Cannot get value of type {type}");
                case ObjectType.UserData:
                    return vm.GetHostObject(idx);
                case ObjectType.Closure:
                    return vm.GetClosure(idx);
                case ObjectType.Instance:
                    return vm.GetInstance(idx);
                case ObjectType.Class:
                    return vm.GetSqClass(idx);
                case ObjectType.Table:
                    return vm.GetTable(idx);
                case ObjectType.Integer:
                    vm.GetInteger(idx, out var iobj);
                    return iobj;
                case ObjectType.Float:
                    vm.GetFloat(idx, out var fobj);
                    return fobj;
                case ObjectType.Bool:
                    vm.GetBool(idx, out var bobj);
                    return bobj;
                case ObjectType.String:
                    vm.GetString(idx, out var sobj);
                    return sobj;
                case ObjectType.Null:
                    return null;
            }
        }

        // = Cores =
        private static void RegisterCores()
        {
            GlobalCore.RegisterCore();
            ConsoleCore.RegisterCore();
        }

        public static class GlobalCore
        {
            public static void RegisterCore()
            {
                SetGlobal("Test", MakeFunction(Test));
                SetGlobal("TextureRegion", MakeFunction(CreateTextureRegion));
                SetGlobal("GetContent", MakeFunction(GetContent));
            }

            public static int Test(Squirrel vm, int argCount)
            {
                if (argCount != 0)
                    throw new Exception();
                return 0;
            }

            public static int CreateTextureRegion(Squirrel vm, int argCount)
            {
                if (argCount == 2)
                {
                    var xy = GetArg(0, VectorClass.RegisteredClass, "Vector");
                    var wh = GetArg(1, VectorClass.RegisteredClass, "Vector");
                    vm.PushHostObject(new TextureRegion(null, new Vector4(
                            xy.Get<float>("X"), xy.Get<float>("Y"), wh.Get<float>("X"), wh.Get<float>("Y")
                        )));
                    return 1;
                }
                else if (argCount == 3)
                {
                    var tex = GetArg<Texture2D>(0);
                    var xy = GetArg(1, VectorClass.RegisteredClass, "Vector");
                    var wh = GetArg(2, VectorClass.RegisteredClass, "Vector");
                    vm.PushHostObject(new TextureRegion(tex, new Vector4(
                            xy.Get<float>("X"), xy.Get<float>("Y"), wh.Get<float>("X"), wh.Get<float>("Y")
                        )));
                    return 1;
                }
                throw ErrorHelper.WrongArgumentCount(argCount, 2, 3);
            }

            public static int GetContent(Squirrel vm, int argCount)
            {
                if (argCount == 1)
                {
                    vm.PushHostObject(Scenes.GameScene.Current.Content.GetLoadedContent<object>(GetArg<string>(0)));
                    return 1;
                }
                throw ErrorHelper.WrongArgumentCount(argCount, 1);
            }
        }

        public static class ConsoleCore
        {
            public static void RegisterCore()
            {
                var module = new SqTable();
                SetGlobal("Console", module);

                module["Write"] = MakeFunction(Write);
                module["WriteLine"] = MakeFunction(WriteLine);
            }

            public static int Write(Squirrel vm, int argCount)
            {
                for (var i = 0; i < argCount; i++)
                    Console.Write(GetArg(i));
                return 0;
            }

            public static int WriteLine(Squirrel vm, int argCount)
            {
                for (var i = 0; i < argCount; i++)
                    Console.Write(GetArg(i));
                Console.WriteLine();
                return 0;
            }
        }

        public static class ErrorHelper
        {
            public static Exception WrongRightOperandType(params string[] expectedTypes)
            {
                return new Exception($"Right-hand operand operand was the wrong type; expected {string.Join("/", expectedTypes)}");
            }

            public static Exception WrongArgumentCount(int count, params int[] expectedCounts)
            {
                return new ArgumentException($"Invalid number of arguments; expected {string.Join("/", expectedCounts)} arguments but got {count}");
            }

            public static Exception WrongArgumentType(int num, string type, params string[] expectedTypes)
            {
                return new ArgumentException($"Argument {num} was the wrong type; expected {string.Join("/", expectedTypes)} but got {type}");
            }

            public static Exception WrongMemberOrSlotType(object key, params string[] expectedTypes)
            {
                return new Exception($"Member/slot {key} was the wrong type; expected {string.Join("/", expectedTypes)}");
            }

            public static Exception WrongType(string name, params string[] expectedTypes)
            {
                return new Exception($"{name} was the wrong type; expected {string.Join("/", expectedTypes)}");
            }

            public static Exception WrongType(string name, params Type[] expectedTypes)
            {
                return new Exception($"{name} was the wrong type; expected {string.Join("/", expectedTypes.Select(e => e.Name))}");
            }
        }
    }

    public static class TypeCheckExtensions
    {
        public static T GetAs<T>(this object obj)
        {
            if (obj is T ret)
                return ret;
            throw ScriptHandler.ErrorHelper.WrongType("Value", typeof(int));
        }
    }
}
