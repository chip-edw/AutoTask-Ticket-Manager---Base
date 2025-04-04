using System.Collections.Specialized;
using System.ComponentModel;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey?, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableDictionary() : base() { }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);

            if (value != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
            else
            {
                return;
            }

        }

        public new bool Remove(TKey key)
        {
            TValue value;
            if (base.TryGetValue(key, out value))
            {
                bool result = base.Remove(key);
                if (result)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
                return result;
            }
            return false;
        }

        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                TValue oldValue;
                bool exists = base.TryGetValue(key, out oldValue);
                base[key] = value;
                if (exists)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
                else
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
