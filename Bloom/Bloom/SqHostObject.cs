using System;
using System.Linq;
using System.Reflection;
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
            private static ulong NextHandleIndex;

            public ulong Index;
            public GCHandle Handle;
            public Squirrel.Unmanaged.SqReleaseHook ReleaseHook;

            public ReferencedObject(SqHostObject hostObj, object obj, out ulong handleIndex)
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
                                Handles[*(ulong*)ptr].Handle.Free();
                                Handles.Remove(*(ulong*)ptr);
                            }
                            return 1;
                        }
                    );
                Index = NextHandleIndex;
                var attemptsToFindNextIndex = 0UL;
                do
                {
                    NextHandleIndex = unchecked(NextHandleIndex + 1);
                    attemptsToFindNextIndex++;
                } while (attemptsToFindNextIndex < ulong.MaxValue && Handles.ContainsKey(NextHandleIndex));
                handleIndex = Index;
                Handles.Add(handleIndex, this);

                hostObj.PushSelf();
                ScriptHandler.Squirrel.SetReleaseHook(-1, ReleaseHook);
                ScriptHandler.Squirrel.Pop(1);
            }
        }

        public static Dictionary<ulong, ReferencedObject> Handles { get; }
            = new Dictionary<ulong, ReferencedObject>();

        public static Dictionary<Type, SqTable> Delegates { get; }
            = new Dictionary<Type, SqTable>();

        public static SqTable GetDelegate(Type type)
        {
            if (Delegates.TryGetValue(type, out var already))
                return already;
            var del = new SqTable();
            // Add methods and properties
            del["_methods"] = MakeMethods(type);
            del["_props"] = MakeProperties(type);
            // Add metamethods for accessing properties/methods
            del["_set"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = (SqHostObject)ScriptHandler.This;
                        var key = ScriptHandler.GetArg(0);
                        var val = ScriptHandler.GetArg(1);
                        // Look for property
                        var props = self["_props"] as SqTable;
                        if (props.TryGetValue(ScriptHandler.GetArg(0), out var prop))
                        {
                            var set = (prop as SqArray)[0] as SqClosure;
                            set.CallAsMethod(self, val);
                            return 0;
                        }
                        // Nothing was found
                        throw new Exception($"Property \"{self.Object.GetType()}.{key}\" does not exist");
                    }
                );
            del["_get"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = (SqHostObject)ScriptHandler.This;
                        var key = ScriptHandler.GetArg(0) as string
                            ?? throw new InvalidOperationException(
                                    $"An object of type {self.Object.GetType().Name} " +
                                    $"must be indexed only by string values"
                                );
                        // Look for method
                        var methods = self["_methods"] as SqTable;
                        if (methods.TryGetValue(ScriptHandler.GetArg(0), out var method))
                        {
                            vm.PushDynamic(method as SqClosure);
                            return 1;
                        }
                        // Look for property
                        var props = self["_props"] as SqTable;
                        if (props.TryGetValue(ScriptHandler.GetArg(0), out var prop))
                        {
                            var get = (prop as SqArray)[1] as SqClosure;
                            vm.PushDynamic(get.CallAsMethod(self)[0]);
                            return 1;
                        }
                        // Nothing was found
                        throw new Exception($"Method/property \"{self.Object.GetType()}.{key}\" does not exist");
                    }
                );
            // Operators
            del["_add"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_Addition", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} + {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["_sub"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_Subtraction", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} - {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["_mul"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_Multiply", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} * {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["_div"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_Division", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} / {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["_modulo"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_Modulus", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} % {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["_unm"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var method = type.GetMethod("op_UnaryNegation", new Type[] { type });
                        if (method is null)
                            throw new Exception($"Operator is not defined: -{type.Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self }));
                        return 1;
                    }
                );
            del["_typeof"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        vm.PushString(self.GetType().Name, -1);
                        return 1;
                    }
                );
            del["_tostring"] = ((SqTable)del["_methods"])["ToString"];
            del["_cmp"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        if (self is IComparable comparableSelf)
                        {
                            vm.PushDynamic(comparableSelf.CompareTo(other));
                            return 1;
                        }
                        var eqMethod = type.GetMethod("op_Equality", new Type[] { type, other.GetType() });
                        if (!(eqMethod is null))
                        {
                            if ((bool)eqMethod.Invoke(null, new object[] { self, other }))
                            {
                                vm.PushDynamic(0);
                                return 1;
                            }
                            var gtMethod = type.GetMethod("op_GreaterThan", new Type[] { type, other.GetType() });
                            if (!(gtMethod is null))
                            {
                                if ((bool)gtMethod.Invoke(null, new object[] { self, other }))
                                {
                                    vm.PushDynamic(1);
                                    return 1;
                                }
                                vm.PushDynamic(-1);
                                return 1;
                            }
                            var ltMethod = type.GetMethod("op_LessThan", new Type[] { type, other.GetType() });
                            if (!(ltMethod is null))
                            {
                                if ((bool)ltMethod.Invoke(null, new object[] { self, other }))
                                {
                                    vm.PushDynamic(-1);
                                    return 1;
                                }
                                vm.PushDynamic(1);
                                return 1;
                            }
                            throw new Exception($"No < or > operator defined for left-hand type {type.Name} " +
                                $"&& right-hand type {other.GetType().Name}");
                        }
                        throw new Exception($"Operator is not defined: {type.Name} == {other.GetType().Name}");
                    }
                );
            del["BOr"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_BitwiseOr", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} | {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["BAnd"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_BitwiseAnd", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} & {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            del["Xor"] = ScriptHandler.MakeFunction(
                    (vm, _) =>
                    {
                        var self = ((SqHostObject)ScriptHandler.This).Object;
                        var other = ScriptHandler.GetArg<object>(0);
                        var method = type.GetMethod("op_ExclusiveOr", new Type[] { type, other.GetType() });
                        if (method is null)
                            throw new Exception($"Operator is not defined: {type.Name} ^ {other.GetType().Name}");
                        vm.PushDynamic(method.Invoke(null, new object[] { self, other }));
                        return 1;
                    }
                );
            return del;
        }

        private static SqTable MakeMethods(Type type)
        {
            var methods = new SqTable();
            // Get all methods by name
            var dict = new Dictionary<string, List<MethodInfo>>();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (dict.TryGetValue(method.Name, out var already))
                {
                    already.Add(method);
                    continue;
                }
                var list = new List<MethodInfo> { method };
                dict.Add(method.Name, list);
            }
            // Convert methods
            foreach (var kvp in dict)
            {
                var name = kvp.Key;
                var overrides = kvp.Value;
                methods[name] = ScriptHandler.MakeFunction(
                        (vm, argCount) =>
                        {
                            var self = (SqHostObject)ScriptHandler.This;
                            var possible = overrides
                                .Where(e => e.GetParameters().Length >= argCount);
                            if (!possible.Any())
                                throw new Exception($"No matching method {type}.{name} that can consume the given arguments");
                            var toCall = possible.OrderBy(e => e.GetParameters().Length).First();
                            var methodParams = toCall.GetParameters();
                            var args = new object[argCount];
                            for (var i = 0; i < argCount; i++)
                                args[i] = ScriptHandler.GetArg(i, methodParams[i].ParameterType, true);
                            try
                            {
                                vm.PushDynamic(toCall.Invoke(self.Object, args));
                            }
                            catch (TargetParameterCountException e)
                            {
                                throw new Exception($"Cannot call \"{name}\" with {argCount} arguments");
                            }
                            return 1;
                        }
                    );
            }
            return methods;
        }

        private static SqTable MakeProperties(Type type)
        {
            var props = new SqTable();
            // Convert type properties
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var methods = new SqArray(2);
                props.Add(prop.Name, methods);
                // 0 = Property set method
                methods[0] = ScriptHandler.MakeFunction(
                        (_, __) =>
                        {
                            var self = (SqHostObject)ScriptHandler.This;
                            var setMethod = prop.GetSetMethod();
                            if (setMethod is null)
                                throw new Exception($"Cannot set property \"{self.Object.GetType()}.{prop.Name}\"");
                            var argType = prop.PropertyType;
                            var val = ScriptHandler.GetArg(0, argType, true);
                            prop.GetSetMethod().Invoke(
                                    (self).Object,
                                    new object[] { val }
                                );
                            return 0;
                        }
                    );
                // 1 = Property get method
                methods[1] = ScriptHandler.MakeFunction(
                        (vm, _) =>
                        {
                            var self = (SqHostObject)ScriptHandler.This;
                            var getMethod = prop.GetGetMethod();
                            if (getMethod is null)
                                throw new Exception($"Cannot get property \"{self.Object.GetType()}.{prop.Name}\"");
                            vm.PushDynamic(
                                   getMethod.Invoke(
                                        (self).Object,
                                        null
                                    )
                                );
                            return 1;
                        }
                    );
            }
            // Convert type fields
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var methods = new SqArray(2);
                props.Add(field.Name, methods);
                // 0 = Field set
                methods[0] = ScriptHandler.MakeFunction(
                        (_, __) =>
                        {
                            var argType = field.FieldType;
                            var val = ScriptHandler.GetArg(0, argType, true);
                            field.SetValue(((SqHostObject)ScriptHandler.This).Object, val);
                            return 0;
                        }
                    );
                // 1 = Field get
                methods[1] = ScriptHandler.MakeFunction(
                        (vm, _) =>
                        {
                            vm.PushDynamic(field.GetValue(((SqHostObject)ScriptHandler.This).Object));
                            return 1;
                        }
                    );
            }
            return props;
        }

        private ulong HandleIndex
        {
            get
            {
                unsafe
                {
                    return *(ulong*)UserDataPointer;
                }
            }
        }

        public object Object => Handles[HandleIndex].Handle.Target;

        public object this[object key]
        {
            get
            {
                PushSelf();
                VM.PushDynamic(key);
                if (!VM.GetFixed(-2).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to get member/slot with key {key}");
                }
                var ret = VM.GetDynamic(-1);
                VM.Pop(2);
                return ret;
            }
            set
            {
                PushSelf();
                VM.PushDynamic(key);
                VM.PushDynamic(value);
                if (!VM.SetFixed(-3).IsOK())
                {
                    VM.PushDynamic(key);
                    VM.PushDynamic(value);
                    if (!VM.NewSlot(-3, false).IsOK())
                    {
                        VM.Pop(1);
                        throw new Exception($"Unable to set member/slot with key {key}");
                    }
                }
                VM.Pop(1);
            }
        }

        public SqHostObject(object obj)
            : base((uint)IntPtr.Size)
        {
            var refObj = new ReferencedObject(this, obj, out var handleIndex);
            unsafe
            {
                *(ulong*)UserDataPointer = handleIndex;
            }

            SetDelegate(GetDelegate(obj.GetType()));
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

        public static implicit operator SqHostObject(SqDotNet.Object obj)
        {
            return new SqHostObject(obj);
        }
    }
}
