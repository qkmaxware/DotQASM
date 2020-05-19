namespace System.Collections.Generic {

public class BijectiveDictionary<T1, T2>: IDictionary<T1, T2> {
    private Dictionary<T1, T2> _forward;
    private Dictionary<T2, T1> _reverse;

    public T2 this[T1 key] { 
        get {
            return _forward[key];
        }
        set {
            _forward[key] = value;
            _reverse[value] = key;
        }
    }

    public T1 this[T2 key] { 
        get {
            return _reverse[key];
        }
        set {
            _reverse[key] = value;
            _forward[value] = key;
        }
    }

    public ICollection<T1> Keys => _forward.Keys;

    public ICollection<T2> Values => _forward.Values;

    public int Count => _forward.Count;

    public bool IsReadOnly => ((ICollection<T1>)_forward).IsReadOnly;

    public BijectiveDictionary() {
        this._forward = new Dictionary<T1, T2>();
        this._reverse = new Dictionary<T2, T1>();
    }

    public BijectiveDictionary(int capacity) {
        this._forward = new Dictionary<T1, T2>(capacity);
        this._reverse = new Dictionary<T2, T1>(capacity);
    }

    public void Add(T1 key, T2 value) {
        this._forward.Add(key, value);
        this._reverse.Add(value, key);
    }

    public void Add(T2 key, T1 value) {
        this._forward.Add(value, key);
        this._reverse.Add(key, value);
    }

    public void Add(KeyValuePair<T1, T2> item) {
        this.Add(item.Key, item.Value);
    }

    public void Add(KeyValuePair<T2, T1> item) {
        this.Add(item.Key, item.Value);
    }

    public void Clear() {
        this._forward.Clear();
        this._reverse.Clear();
    }

    public void Swap(T1 key1, T1 key2) {
        var tmp1 = this[key1];
        var tmp2 = this[key2];
        this[key1] = tmp2;
        this[key2] = tmp1;
    }

    public void Swap(T2 key1, T2 key2) {
        var tmp1 = this[key1];
        var tmp2 = this[key2];
        this[key1] = tmp2;
        this[key2] = tmp1;
    }

    public bool Contains(KeyValuePair<T1, T2> item) {
        return ((IDictionary<T1, T2>)this._forward).Contains(item);
    }

    public bool Contains(KeyValuePair<T2, T1> item) {
        return ((IDictionary<T2, T1>)this._reverse).Contains(item);
    }

    public bool ContainsKey(T1 key) {
        return this._forward.ContainsKey(key);
    }

    public bool ContainsKey(T2 key) {
        return this._reverse.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex) {
        ((IDictionary<T1, T2>)this._forward).CopyTo(array, arrayIndex);
    }

    public void CopyTo(KeyValuePair<T2, T1>[] array, int arrayIndex) {
        ((IDictionary<T2, T1>)this._reverse).CopyTo(array, arrayIndex);
    }

    public bool Remove(T1 key) {
        if (this._forward.ContainsKey(key)) {
            var k2 = this._forward[key];
            bool a = this._forward.Remove(key);
            bool b = this._reverse.Remove(k2);
            return a && b;
        } else {
            return false;
        }
    }

    public bool Remove(T2 key) {
        if (this._reverse.ContainsKey(key)) {
            var k2 = this._reverse[key];
            bool a = this._reverse.Remove(key);
            bool b = this._forward.Remove(k2);
            return a && b;
        } else {
            return false;
        }
    }

    public bool Remove(KeyValuePair<T1, T2> item) {
        if (((IDictionary<T1, T2>)this._forward).Contains(item)) {
            bool a = this._forward.Remove(item.Key);
            bool b = this._reverse.Remove(item.Value);
            return a && b;
        } else {
            return false;
        }
    }

    public bool Remove(KeyValuePair<T2, T1> item) {
        if (((IDictionary<T2, T1>)this._reverse).Contains(item)) {
            bool a = this._reverse.Remove(item.Key);
            bool b = this._forward.Remove(item.Value);
            return a && b;
        } else {
            return false;
        }
    }

    public bool TryGetValue(T1 key, out T2 value) {
        return this._forward.TryGetValue(key, out value);
    }

    public bool TryGetValue(T2 key, out T1 value) {
        return this._reverse.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
        return this._forward.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this._forward.GetEnumerator();
    }
}

}