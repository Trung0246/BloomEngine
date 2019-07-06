using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SqDotNet;
using Bloom.Handlers;

namespace Bloom
{
    public class SqTable : SqObject, IDictionary<object, object>
    {
        bool ICollection<KeyValuePair<object, object>>.IsReadOnly => false;

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

        ICollection<object> IDictionary<object, object>.Keys => (ICollection<object>)Pairs.Select(e => e.Key);

        ICollection<object> IDictionary<object, object>.Values => (ICollection<object>)Pairs.Select(e => e.Value);

        public IEnumerable<KeyValuePair<object, object>> Pairs
        {
            get
            {
                var pairs = new List<KeyValuePair<object, object>>();
                PushSelf();
                VM.PushNull();
                while (VM.Next(-2).IsOK())
                {
                    var key = VM.GetDynamic(-2);
                    var value = VM.GetDynamic(-1);
                    pairs.Add(new KeyValuePair<object, object>(key, value));
                    VM.Pop(2);
                }
                VM.Pop(1);
                return pairs;
            }
        }

        public object this[object key]
        {
            get
            {
                PushSelf();
                VM.PushDynamic(key);
                if (!VM.GetFixed(-2).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to read slot with key {key}");
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
                if (!VM.NewSlot(-3, false).IsOK())
                {
                    VM.Pop(1);
                    throw new Exception($"Unable to create/set slot with key {key}");
                }
                VM.Pop(1);
            }
        }

        public SqTable() : this(GenerateTableRef(ScriptHandler.Squirrel))
        {
            VM.Pop(1);
        }

        public SqTable(SqDotNet.Object tableRef)
            : base(ScriptHandler.Squirrel, tableRef)
        {
        }

        private static SqDotNet.Object GenerateTableRef(Squirrel vm)
        {
            vm.NewTable();
            vm.GetStackObj(-1, out var obj);
            return obj;
        }

        public void Add(object key, object value)
        {
            if (ContainsKey(key))
                throw new InvalidOperationException($"Table already contains the key {key}");
            this[key] = value;
        }

        public void Add(KeyValuePair<object, object> pair)
        {
            Add(pair.Key, pair.Value);
        }

        public bool ContainsKey(object key)
        {
            PushSelf();
            VM.PushDynamic(key);
            var found = VM.GetFixed(-2).IsOK();
            if (found)
            {
                VM.Pop(2);
                return true;
            }
            VM.Pop(1);
            return false;
        }

        public bool Contains(KeyValuePair<object, object> pair)
        {
            return TryGetValue(pair.Key, out var foundValue) && pair.Value == foundValue;
        }

        public bool TryGetValue(object key, out object value)
        {
            PushSelf();
            VM.PushDynamic(key);
            var found = VM.GetFixed(-2).IsOK();
            if (found)
            {
                value = VM.GetDynamic(-1);
                VM.Pop(2);
                return true;
            }
            VM.Pop(1);
            value = default;
            return false;
        }

        public bool Remove(object key)
        {
            PushSelf();
            VM.PushDynamic(key);
            var deleted = VM.DeleteSlot(-2, false).IsOK();
            VM.Pop(1);
            return deleted;
        }

        public bool Remove(KeyValuePair<object, object> pair)
        {
            if (Contains(pair))
                return Remove(pair.Key);
            return false;
        }

        public void Clear()
        {
            PushSelf();
            VM.Clear(-1);
            VM.Pop(1);
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (array.Length - index < Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int count = Count;
            foreach (var pair in Pairs)
            {
                array[index++] = pair;
            }
        }

        IEnumerator<KeyValuePair<object, object>> IEnumerable<KeyValuePair<object, object>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SqTableEnumerator GetEnumerator()
        {
            return new SqTableEnumerator(Pairs);
        }
    }

    public class SqTableEnumerator : IEnumerator<KeyValuePair<object, object>>
    {
        public KeyValuePair<object, object>[] Values;

        private int Position = -1;

        public SqTableEnumerator(IEnumerable<KeyValuePair<object, object>> values)
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

        public KeyValuePair<object, object> Current
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
