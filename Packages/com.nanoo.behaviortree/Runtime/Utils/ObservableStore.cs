using System;
using System.Collections.Generic;

namespace Utils
{
    public interface IObservableStore
    {
        public void Subscribe(string varName, Action<object> callback);
        public void Unsubscribe(string varName, Action<object> callback);

        public void SetValue<T>(string key, T data) where T : struct;
        public void SetRef<T>(string key, T data) where T : class;
        public T Get<T>(string key);
        public bool Has(string key);
    }

    public class SimpleObservableStore : IObservableStore
    {
        private readonly Dictionary<string, object> _store = new();
        private readonly Dictionary<string, Action<object>> _callbacks = new();

        public void Subscribe(string varName, Action<object> callback)
        {
            if (!_callbacks.TryAdd(varName, callback))
                _callbacks[varName] += callback;
        }

        public void Unsubscribe(string varName, Action<object> callback)
        {
            if (_callbacks.ContainsKey(varName))
                _callbacks[varName] -= callback;
        }

        private void Set<T>(string key, T data)
        {
            if (!_store.TryAdd(key, data))
            {
                if (EqualityComparer<T>.Default.Equals((T)_store[key], data)) return;
                _store[key] = data;
            }
            if (_callbacks.TryGetValue(key, out Action<object> callback)) callback?.Invoke(data);
        }

        public bool Has(string key) => _store.ContainsKey(key);
        public void SetValue<T>(string key, T data) where T : struct => Set(key, data);
        public void SetRef<T>(string key, T data) where T : class => Set(key, data);
        public T Get<T>(string key)
        {
            if (_store.TryGetValue(key, out var data))
                return (T)data;
            return default;
        }
    }
}
