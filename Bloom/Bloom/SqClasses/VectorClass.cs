using System;
using Bloom.Handlers;
using SqDotNet;

namespace Bloom.SqClasses
{
    public class VectorClass : SqClass
    {
        public static VectorClass RegisteredClass { get; private set; }

        private VectorClass() : base("Vector")
        {
            NewField("X", 0f);
            NewField("Y", 0f);
            NewField("Z", 0f);
            NewField("W", 0f);

            SetConstructor(ScriptHandler.MakeFunction(Constructor));

            NewMethod("_tostring", ScriptHandler.MakeFunction(ToString));
            NewMethod("_add", ScriptHandler.MakeFunction(Add));
            NewMethod("_sub", ScriptHandler.MakeFunction(Sub));
            NewMethod("_mul", ScriptHandler.MakeFunction(Mul));
            NewMethod("_div", ScriptHandler.MakeFunction(Div));
            NewMethod("_modulo", ScriptHandler.MakeFunction(Modulo));
            NewMethod("_unm", ScriptHandler.MakeFunction(Unm));

            NewMethod("Normalized", ScriptHandler.MakeFunction(Normalized));
            NewMethod("Length", ScriptHandler.MakeFunction(Length));
        }

        public static void Register()
        {
            RegisteredClass = new VectorClass();
            ScriptHandler.SetGlobal("Vector", RegisteredClass);
        }

        public static int Constructor(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            self["X"] = ScriptHandler.GetArg<float>(0);
            self["Y"] = ScriptHandler.GetArg<float>(1);
            self["Z"] = ScriptHandler.GetArg<float>(2);
            self["W"] = ScriptHandler.GetArg<float>(3);
            return 0;
        }

        public static int ToString(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            vm.PushString($"{{{self["X"]}, {self["Y"]}, {self["Z"]}, {self["W"]}}}", -1);
            return 1;
        }

        public static int Add(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var other = ScriptHandler.GetArg(0);
            if (other is SqInstance otherVector)
            {
                if (!otherVector.IsInstanceOf(RegisteredClass))
                {
                    throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
                }
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") + otherVector.Get<float>("X"),
                        self.Get<float>("Y") + otherVector.Get<float>("Y"),
                        self.Get<float>("Z") + otherVector.Get<float>("Z"),
                        self.Get<float>("W") + otherVector.Get<float>("W")
                    ));
            }
            else if (other is float || other is int)
            {
                float otherFloat;
                if (other is float)
                    otherFloat = (float)other;
                else
                    otherFloat = (int)other;
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") + otherFloat,
                        self.Get<float>("Y") + otherFloat,
                        self.Get<float>("Z") + otherFloat,
                        self.Get<float>("W") + otherFloat
                    ));
            }
            else
                throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
            return 1;
        }

        public static int Sub(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var other = ScriptHandler.GetArg(0);
            if (other is SqInstance otherVector)
            {
                if (!otherVector.IsInstanceOf(RegisteredClass))
                {
                    throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
                }
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") - otherVector.Get<float>("X"),
                        self.Get<float>("Y") - otherVector.Get<float>("Y"),
                        self.Get<float>("Z") - otherVector.Get<float>("Z"),
                        self.Get<float>("W") - otherVector.Get<float>("W")
                    ));
            }
            else if (other is float || other is int)
            {
                float otherFloat;
                if (other is float)
                    otherFloat = (float)other;
                else
                    otherFloat = (int)other;
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") - otherFloat,
                        self.Get<float>("Y") - otherFloat,
                        self.Get<float>("Z") - otherFloat,
                        self.Get<float>("W") - otherFloat
                    ));
            }
            else
                throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
            return 1;
        }

        public static int Mul(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var other = ScriptHandler.GetArg(0);
            if (other is SqInstance otherVector)
            {
                if (!otherVector.IsInstanceOf(RegisteredClass))
                {
                    throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
                }
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") * otherVector.Get<float>("X"),
                        self.Get<float>("Y") * otherVector.Get<float>("Y"),
                        self.Get<float>("Z") * otherVector.Get<float>("Z"),
                        self.Get<float>("W") * otherVector.Get<float>("W")
                    ));
            }
            else if (other is float || other is int)
            {
                float otherFloat;
                if (other is float)
                    otherFloat = (float)other;
                else
                    otherFloat = (int)other;
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") * otherFloat,
                        self.Get<float>("Y") * otherFloat,
                        self.Get<float>("Z") * otherFloat,
                        self.Get<float>("W") * otherFloat
                    ));
            }
            else
                throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
            return 1;
        }

        public static int Div(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var other = ScriptHandler.GetArg(0);
            if (other is SqInstance otherVector)
            {
                if (!otherVector.IsInstanceOf(RegisteredClass))
                {
                    throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
                }
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") / otherVector.Get<float>("X"),
                        self.Get<float>("Y") / otherVector.Get<float>("Y"),
                        self.Get<float>("Z") / otherVector.Get<float>("Z"),
                        self.Get<float>("W") / otherVector.Get<float>("W")
                    ));
            }
            else if (other is float || other is int)
            {
                float otherFloat;
                if (other is float)
                    otherFloat = (float)other;
                else
                    otherFloat = (int)other;
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") / otherFloat,
                        self.Get<float>("Y") / otherFloat,
                        self.Get<float>("Z") / otherFloat,
                        self.Get<float>("W") / otherFloat
                    ));
            }
            else
                throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
            return 1;
        }

        public static int Modulo(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var other = ScriptHandler.GetArg(0);
            if (other is SqInstance otherVector)
            {
                if (!otherVector.IsInstanceOf(RegisteredClass))
                {
                    throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
                }
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") % otherVector.Get<float>("X"),
                        self.Get<float>("Y") % otherVector.Get<float>("Y"),
                        self.Get<float>("Z") % otherVector.Get<float>("Z"),
                        self.Get<float>("W") % otherVector.Get<float>("W")
                    ));
            }
            else if (other is float || other is int)
            {
                float otherFloat;
                if (other is float)
                    otherFloat = (float)other;
                else
                    otherFloat = (int)other;
                vm.PushDynamic(RegisteredClass.CallConstructor(
                        self.Get<float>("X") % otherFloat,
                        self.Get<float>("Y") % otherFloat,
                        self.Get<float>("Z") % otherFloat,
                        self.Get<float>("W") % otherFloat
                    ));
            }
            else
                throw ScriptHandler.ErrorHelper.WrongRightOperandType("Vector", "Float", "Integer");
            return 1;
        }

        public static int Unm(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            vm.PushDynamic(RegisteredClass.CallConstructor(
                    -self.Get<float>("X"),
                    -self.Get<float>("Y"),
                    -self.Get<float>("Z"),
                    -self.Get<float>("W")
                ));
            return 1;
        }

        public static int Normalized(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var x = self.Get<float>("X");
            var y = self.Get<float>("Y");
            var z = self.Get<float>("Z");
            var w = self.Get<float>("W");
            var len = MathF.Sqrt(x * x + y * y + z * z + w * w);
            vm.PushDynamic(RegisteredClass.CallConstructor(
                    x / len,
                    y / len,
                    z / len,
                    w / len
                ));
            return 1;
        }

        public static int Length(Squirrel vm, int argCount)
        {
            var self = ScriptHandler.This;
            var x = self.Get<float>("X");
            var y = self.Get<float>("Y");
            var z = self.Get<float>("Z");
            var w = self.Get<float>("W");
            vm.PushFloat(MathF.Sqrt(x * x + y * y + z * z + w * w));
            return 1;
        }
    }
}
