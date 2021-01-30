using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace blazor_client.Reactivity
{
    public class ReactiveList<T> : BaseNotifyPropertyChanged, IList<T>, INotifyCollectionChanged
    {
        public ReactiveList()
        {
            _shouldSetupPropertyChanges = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
        }

        private List<T> _internal = new List<T>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool _shouldSetupPropertyChanges = false;
        private void Subitem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine($"[ReactiveList] Property {e.PropertyName} changed");
            NotifyStateChanged(e.PropertyName);
        }

        private void TrySetupPropertyChange(T item)
        {
            if (_shouldSetupPropertyChanges)
            {
                var property = (INotifyPropertyChanged)item;
                if (property == null) return;
                
                // subtract so we don't have duplicate registrations
                property.PropertyChanged -= Subitem_PropertyChanged;
                property.PropertyChanged += Subitem_PropertyChanged;
            }
        }

        private void TryUnsetPropertyChange(T item)
        {
            if (_shouldSetupPropertyChanges)
            {
                var property = (INotifyPropertyChanged)item;
                if (property == null) return;

                property.PropertyChanged -= Subitem_PropertyChanged;
            }
        }

        public T this[int index]
        {
            get => _internal[index];
            set
            {
                var old = _internal[index];
                TryUnsetPropertyChange(old);

                _internal[index] = value;
                TrySetupPropertyChange(value);
            }
        }

        public int Count => _internal.Count;
        public bool IsReadOnly => false;
        public bool Contains(T item) => _internal.Contains(item);
        public IEnumerator<T> GetEnumerator() => _internal.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();
        public int IndexOf(T item) => _internal.IndexOf(item);
        public void CopyTo(T[] array, int arrayIndex) => _internal.CopyTo(array, arrayIndex);

        public void AddMultiple(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _internal.Add(item);
                TrySetupPropertyChange(item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            NotifyStateChanged();
        }

        public void Add(T item)
        {
            _internal.Add(item);
            TrySetupPropertyChange(item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            NotifyStateChanged();
        }

        public void Clear()
        {
            if (_shouldSetupPropertyChanges)
            {
                foreach (var v in _internal)
                {
                    TryUnsetPropertyChange(v);
                }
            }

            _internal.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            NotifyStateChanged();
        }

        public void Insert(int index, T item)
        {
            _internal.Insert(index, item);
            TrySetupPropertyChange(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            NotifyStateChanged();
        }

        public bool Remove(T item)
        {
            var res = _internal.Remove(item);
            if (res) TryUnsetPropertyChange(item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            NotifyStateChanged();
            return res;
        }

        public void RemoveAt(int index)
        {
            TryUnsetPropertyChange(_internal[index]);

            _internal.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            NotifyStateChanged();
        }
    }
}
