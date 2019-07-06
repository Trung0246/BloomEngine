﻿using SqDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using WyvernFramework;

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

            PushCompiledFile("hello.nut");
            PopToCall();
            CallGlobal("main");
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
            Debug.Error(message, "Squirrel");
        }

        public static bool IsOK(this int result)
        {
            return result == 0;
        }

        /// <summary>
        /// Push a slot from the current root table
        /// </summary>
        /// <param name="key"></param>
        private static void PushGlobal(string key)
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
        private static object GetGlobal(string key)
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
        private static void PopToGlobal(string key)
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
        private static void SetGlobal(string key, Function obj)
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
        private static void SetGlobal(string key, object obj)
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
        private static void PushCompiledString(string source, string name)
        {
            if (!Squirrel.CompileBuffer(source, source.Length, name, true).IsOK())
                throw new Exception($"Unable to compile script \"{name}\"");
        }

        /// <summary>
        /// Compile a source then push the function
        /// </summary>
        /// <param name="path"></param>
        private static void PushCompiledFile(string path)
        {
            PushCompiledString(File.ReadAllText(Path.Combine(ScriptRoot, path)), path);
        }

        /// <summary>
        /// Pop function then call it
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static object[] PopToCall(params object[] parameters)
        {
            var top = Squirrel.GetTop();
            Squirrel.PushRootTable();
            foreach (var obj in parameters)
                Squirrel.PushDynamic(obj);
            if (!Squirrel.Call(parameters.Length + 1, true, true).IsOK())
                throw new InvalidOperationException("Unable to call the closure");
            var returnCount = Squirrel.GetTop() - top;
            var returns = new object[returnCount];
            var idx = top + 1;
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
        private static object[] CallGlobal(string key, params object[] parameters)
        {
            PushGlobal(key);
            return PopToCall(parameters);
        }

        /// <summary>
        /// Make a Squirrel function
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static Function MakeFunction(Func<Squirrel, int, int> func)
        {
            return new Function((vm) => func(vm, vm.GetTop() - 1));
        }

        /// <summary>
        /// Get an argument (if in a called function)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private static object GetArg(int idx)
        {
            return Squirrel.GetDynamic(2 + idx);
        }

        /// <summary>
        /// The stack index at which, during a call, the environment object ("this") should be
        /// </summary>
        private static int ThisIndex => 1;

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
        /// Push a table on the stack
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="table"></param>
        public static void PushTable(this Squirrel vm, SqTable table)
        {
            vm.PushObject(table.ObjectRef);
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
                    throw new InvalidOperationException($"Cannot push value of type {obj.GetType().Name}");
                case nameof(Byte):
                    vm.PushInteger((byte)(object)obj);
                    break;
                case nameof(Int16):
                    vm.PushInteger((short)(object)obj);
                    break;
                case nameof(Int32):
                    vm.PushInteger((int)(object)obj);
                    break;
                case nameof(Int64):
                    vm.PushInteger((int)(long)(object)obj);
                    break;
                case nameof(IntPtr):
                    vm.PushUserPointer((IntPtr)(object)obj);
                    break;
                case nameof(Single):
                    vm.PushFloat((float)(object)obj);
                    break;
                case nameof(Double):
                    vm.PushFloat((float)(double)(object)obj);
                    break;
                case nameof(Boolean):
                    vm.PushBool((bool)(object)obj);
                    break;
                case nameof(String):
                    vm.PushString((string)(object)obj, -1);
                    break;
                case nameof(SqTable):
                    vm.PushTable((SqTable)(object)obj);
                    break;
                case nameof(Function):
                    Squirrel.NewClosure((Function)(object)obj, 0);
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
            ConsoleCore.RegisterCore();
        }

        public static class ConsoleCore
        {
            public static void RegisterCore()
            {
                var module = new SqTable();
                SetGlobal("Console", module);

                module["Write"] = MakeFunction(ConsoleCore.Write);
                module["WriteLine"] = MakeFunction(ConsoleCore.WriteLine);
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
    }
}
