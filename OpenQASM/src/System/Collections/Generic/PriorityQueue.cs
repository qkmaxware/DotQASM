namespace System.Collections.Generic {

/// <summary>
/// Priority queue 
/// </summary>
/// <typeparam name="T">stored type</typeparam>
public class PriorityQueue<T> {

    private List<T> data; 
    private IComparer<T> comparer;

    public int Count => data.Count; // O(1)
    public bool IsEmpty => Count == 0; // O(1)

    public PriorityQueue(IComparer<T> comparer) {
        this.data = new List<T>();
        this.comparer = comparer;
    }

    public PriorityQueue(int cap, IComparer<T> comparer) {
        this.data = new List<T>(cap);
        this.comparer = comparer;
    }

    public T Root() {
        return data[0];
    } // O(1)

    public T Dequeue() {
        if (IsEmpty)
            return default(T);

        int li = data.Count - 1;
        T root = Root();
        data[0] = data[li];
        data.RemoveAt(li);

        --li;
        int pi = 0;
        while (true) {
            int ci = pi * 2 + 1;
            if (ci  > li) {
                break;
            }
            int rc = ci + 1;
            if (rc  <= li && comparer.Compare(data[rc], data[ci])  < 0) {
                ci = rc;
            }
            if (comparer.Compare(data[pi],data[ci])  <= 0) {
                break;
            }
            swap(pi, ci);
            pi = ci; 
        }

        return root;
    }
    
    private void swap(int i, int j) {
        T temp = data[i];
        data[i] = data[j];
        data[j] = temp;
    }

    public void Enqueue(T value) {
        data.Add(value);
        int ci = data.Count - 1;
        while (ci > 0) {
            int pi = (ci - 1) / 2; // (ci - 1) >> 1;
            if (comparer.Compare(data[ci], data[pi]) >= 0) {
                break;
            }
            swap(ci, pi);
            ci = pi;
        }
    } 

    public bool Contains(T value) {
        return data.Contains(value);
    }

    public bool ContainsEquivalent(T value, out T element) {
        int ind = data.IndexOf(value);
        if (ind < 0) {
            element = default(T);
            return false;
        } else {
            element = data[ind];
            return true;
        }
    }
}

}