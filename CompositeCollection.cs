using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TankIconMaker
{
    class CompositeCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private List<IList<T>> _collections = new List<IList<T>>();

        public bool IsReadOnly { get { return true; } }
        public int Count { get; private set; }

        public void Clear()
        {
            _collections.Clear();
            Count = 0;
            propertyChanged("Count");
            collectionChanged_Reset();
        }

        public void AddCollection<TC>(TC collection)
            where TC : IList<T>, INotifyCollectionChanged
        {
            _collections.Add(collection);
            collection.CollectionChanged += collectionChanged;
            int offset = Count;
            Count += collection.Count;
            propertyChanged("Count");
            if (collection.Count > 5)
                collectionChanged_Reset();
            else
                for (int i = 0; i < collection.Count; i++)
                    collectionChanged_Added(collection[i], offset + i);
        }

        public bool Contains(T item)
        {
            foreach (var coll in _collections)
                if (coll.Contains(item))
                    return true;
            return false;
        }

        public int IndexOf(T item)
        {
            int offset = 0;
            foreach (var coll in _collections)
            {
                int index = coll.IndexOf(item);
                if (index >= 0)
                    return offset + index;
                offset += coll.Count;
            }
            return -1;
        }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();
                foreach (var coll in _collections)
                {
                    if (index < coll.Count)
                        return coll[index];
                    index -= coll.Count;
                }
                throw new IndexOutOfRangeException();
            }
            set { throw new NotSupportedException(); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var coll in _collections)
                foreach (var item in coll)
                    yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }

        public void Insert(int index, T item) { throw new NotSupportedException(); }
        public void RemoveAt(int index) { throw new NotSupportedException(); }
        public void Add(T item) { throw new NotSupportedException(); }
        public bool Remove(T item) { throw new NotSupportedException(); }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Count = Count + (e.NewItems == null ? 0 : e.NewItems.Count) - (e.OldItems == null ? 0 : e.OldItems.Count);
            propertyChanged("Count");

            if (CollectionChanged == null)
                return;

            int offset = 0;
            foreach (var coll in _collections)
                if (sender == coll)
                    break;
                else
                    offset += coll.Count;
            var newIndex = e.NewStartingIndex == -1 ? -1 : (e.NewStartingIndex + offset);
            var oldIndex = e.OldStartingIndex == -1 ? -1 : (e.OldStartingIndex + offset);
            // Fffffuuuuu....
            NotifyCollectionChangedEventArgs args;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                case NotifyCollectionChangedAction.Add:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, newIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, newIndex, oldIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, newIndex);
                    break;
                default:
                    throw new Exception("bug");
            }
            CollectionChanged(this, args);
        }

        private void collectionChanged_Reset()
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void collectionChanged_Added(T item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        private void collectionChanged_Removed(T item, int index)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        private void collectionChanged_Moved(T item, int oldIndex, int newIndex)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        private void propertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
