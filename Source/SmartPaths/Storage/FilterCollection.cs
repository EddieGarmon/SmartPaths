using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace SmartPaths.Storage;

internal sealed class FilterCollection : Collection<string>
{

    private readonly Dictionary<string, Func<string, bool>> _patterns = [];

    internal FilterCollection()
        : base(new ImmutableStringList()) { }

    internal bool IsMatch(string? name) {
        if (name is null) {
            return false;
        }
        return Items.Count == 0 || Items.Select(filter => _patterns[filter]).Any(isMatch => isMatch(name));
    }

    protected override void InsertItem(int index, string item) {
        item = string.IsNullOrEmpty(item) || item == "*.*" ? "*" : item;
        if (!_patterns.ContainsKey(item)) {
            MakePattern(item);
        }
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, string item) {
        item = string.IsNullOrEmpty(item) || item == "*.*" ? "*" : item;
        if (!_patterns.ContainsKey(item)) {
            MakePattern(item);
        }
        base.SetItem(index, item);
    }

    private void MakePattern(string expression) {
        if (expression is "*" or "*.*") {
            _patterns[expression] = AlwaysTrue;
        }

        ReadOnlySpan<char> span = expression.AsSpan();
        if (span[0] == '*') {
            ReadOnlySpan<char> end = span.Slice(1);
            ReadOnlySpan<char> star = "*".AsSpan();
            ReadOnlySpan<char> questionMark = "?".AsSpan();
            bool hasWildcard = end.Contains(star, StringComparison.OrdinalIgnoreCase) || //
                               end.Contains(questionMark, StringComparison.OrdinalIgnoreCase);
            if (!hasWildcard) {
                // match the end
                _patterns[expression] = name => MatchEnd(expression, name);
            }
        }
        //transmute to Regex
        Regex regex = new("^" + Regex.Escape(expression).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        _patterns[expression] = name => regex.IsMatch(name);
    }

    private static bool AlwaysTrue(string _) {
        return true;
    }

    private static bool MatchEnd(string expression, string name) {
        ReadOnlySpan<char> end = expression.AsSpan(1);
        if (name.Length < end.Length) {
            return false;
        }
        return name.AsSpan().EndsWith(end, StringComparison.OrdinalIgnoreCase);
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