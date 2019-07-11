using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SqDotNet;
using Bloom.Handlers;

namespace Bloom
{
    public class SqArray : SqObject, IList<object>, IEquatable<object>
    {
        bool ICollection<object>.IsReadOnly => false;

        public int Count
        {
            get
            {
                PushSelf();
                var count = VM.GetSize(VM.GetTop());
                VM.Pop(1);
                return count;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                var values = new List<object>();
                PushSelf();
                VM.PushNull();
                while (VM.Next(VM.GetTop() - 1).IsOK())
                {
                    var value = VM.GetDynamic(-1);
                    values.Add(value);
                    VM.Pop(2);
                }
                VM.Pop(1);
                return values;
            }
        }

        public IEnumerable<KeyValuePair<int, object>> Pairs
        {
            get
            {
                var pairs = new List<KeyValuePair<int, object>>();
                PushSelf();
                VM.PushNull();
                while (VM.Next(VM.GetTop() - 1).IsOK())
                {
                    VM.GetInteger(-2, out var key);
                    var value = VM.GetDynamic(-1);
                    pairs.Add(new KeyValuePair<int, object>(key, value));
                    VM.Pop(2);
                }
                VM.Pop(1);
                return pairs;
            }
        }

        public object this[int key]
        {
            get
            {
                PushSelf();
                VM.PushInteger(key);
                if (!VM.GetFixed(-2).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Slot {key} was outside the array (size: {Count})");
                }
                var ret = VM.GetDynamic(-1);
                VM.Pop(2);
                return ret;
            }
            set
            {
                PushSelf();
                VM.PushInteger(key);
                VM.PushDynamic(value);
                if (!VM.SetFixed(-3).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Slot {key} was outside the array (size: {Count})");
                }
                VM.Pop(1);
            }
        }

        public SqArray(int size) : this(GenerateArrayRef(ScriptHandler.Squirrel, size))
        {
            VM.Pop(1);
        }

        public SqArray(SqDotNet.Object arrayRef)
            : base(ScriptHandler.Squirrel, arrayRef)
        {
        }

        private static SqDotNet.Object GenerateArrayRef(Squirrel vm, int size)
        {
            vm.NewArray(size);
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public int IndexOf(object obj)
        {
            foreach (var pair in Pairs)
            {
                if (pair.Value == obj)
                    return pair.Key;
            }
            return -1;
        }

        public bool Contains(object obj)
        {
            foreach (var value in Values)
            {
                if (value == obj)
                    return true;
            }
            return false;
        }

        public void CopyTo(object[] array, int index)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (array.Length - index < Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            foreach (var pair in Pairs)
                array[index + pair.Key] = pair.Value;
        }

        void IList<object>.Insert(int index, object item)
        {
            throw new NotSupportedException("SqArray has a fixed size");
        }

        bool ICollection<object>.Remove(object item)
        {
            throw new NotSupportedException("SqArray has a fixed size");
        }

        void ICollection<object>.Add(object item)
        {
            throw new NotSupportedException("SqArray has a fixed size");
        }

        void ICollection<object>.Clear()
        {
            throw new NotSupportedException("SqArray has a fixed size");
        }

        void IList<object>.RemoveAt(int index)
        {
            throw new NotSupportedException("SqArray has a fixed size");
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SqArrayEnumerator GetEnumerator()
        {
            return new SqArrayEnumerator(Values);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqObject);
        }

        public static bool operator ==(SqArray left, SqArray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SqArray left, SqArray right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Values.Select(e => (e is null) ? "null" : e.ToString()))}]";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator SqArray(SqDotNet.Object obj)
        {
            return new SqArray(obj);
        }
    }

    public class SqArrayEnumerator : IEnumerator<object>
    {
        public object[] Values;

        private int Position = -1;

        public SqArrayEnumerator(IEnumerable<object> values)
        {
            Values = values.ToArray();
        }

        public bool MoveNext()
        {
            Position++;
            return Position < Values.Length;
        }

        public void Reset()
        {
            Position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public object Current
        {
            get
            {
                try
                {
                    return Values[Position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
