using System.Collections;
using System.Collections.ObjectModel;

namespace SmartPaths.Storage;

internal sealed class FilterCollection : Collection<string>
{

    internal FilterCollection()
        : base(new ImmutableStringList()) { }

    internal string[] GetFilters() {
        return ((ImmutableStringList)Items).Items;
    }

    protected override void InsertItem(int index, string item) {
        base.InsertItem(index, string.IsNullOrEmpty(item) || item == "*.*" ? "*" : item);
    }

    protected override void SetItem(int index, string item) {
        base.SetItem(index, string.IsNullOrEmpty(item) || item == "*.*" ? "*" : item);
    }

    /// <summary>List that maintains its underlying data in an immutable array, such that the list will
    ///     never modify an array returned from its Items property. This is to allow the array to be
    ///     enumerated safely while another thread might be concurrently mutating the collection.</summary>
    private sealed class ImmutableStringList : IList<string>
    {

        public string[] Items = [];

        public int Count => Items.Length;

        public bool IsReadOnly => false;

        public string this[int index] {
            get {
                string[] items = Items;
                if (index >= items.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return items[index];
            }
            set {
                string[] clone = (string[])Items.Clone();
                clone[index] = value;
                Items = clone;
            }
        }

        public void Add(string item) {
            // Collection<T> doesn't use this method.
            throw new NotSupportedException();
        }

        public void Clear() {
            Items = [];
        }

        public bool Contains(string item) {
            return Array.IndexOf(Items, item) != -1;
        }

        public void CopyTo(string[] array, int arrayIndex) {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator() {
            return ((IEnumerable<string>)Items).GetEnumerator();
        }

        public int IndexOf(string item) {
            return Array.IndexOf(Items, item);
        }

        public void Insert(int index, string item) {
            string[] items = Items;
            string[] newItems = new string[items.Length + 1];
            items.AsSpan(0, index).CopyTo(newItems);
            items.AsSpan(index).CopyTo(newItems.AsSpan(index + 1));
            newItems[index] = item;
            Items = newItems;
        }

        public bool Remove(string item) {
            // Collection<T> doesn't use this method.
            throw new NotSupportedException();
        }

        public void RemoveAt(int index) {
            string[] items = Items;
            string[] newItems = new string[items.Length - 1];
            items.AsSpan(0, index).CopyTo(newItems);
            items.AsSpan(index + 1).CopyTo(newItems.AsSpan(index));
            Items = newItems;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }

}