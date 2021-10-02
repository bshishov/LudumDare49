using System;
using System.Collections.Generic;

namespace Utils
{
    internal interface IHandler<in T>
    {
        void TryHandle(T obj);
        bool IsValid();
    }

    internal class Handler<T, TBase> : IHandler<TBase>
        where T : TBase
    {
        private readonly Action<T> _action;

        public Handler(Action<T> action)
        {
            _action = action;
        }

        public void TryHandle(TBase obj)
        {
            if (obj is T value)
                _action.Invoke(value);
        }

        public bool IsValid()
        {
            return _action != null;
        }
    }


    public class TypedEvent<TBase>
    {
        private readonly Dictionary<Type, List<IHandler<TBase>>> _handlers =
            new Dictionary<Type, List<IHandler<TBase>>>();

        public void AddListener<T>(Action<T> action)
            where T : TBase
        {
            var t = typeof(T);
            var handler = new Handler<T, TBase>(action);
            if (_handlers.ContainsKey(t))
                _handlers[t].Add(handler);
            else
                _handlers.Add(t, new List<IHandler<TBase>> {handler});
        }

        public void Invoke<T>(T obj)
            where T : class, TBase
        {
            Invoke(typeof(T), obj);
        }

        public void Invoke(Type t, TBase obj)
        {
            if (_handlers.TryGetValue(t, out var handlers))
                for (var i = handlers.Count - 1; i >= 0; i--)
                {
                    var handler = handlers[i];
                    if (handler.IsValid())
                        handler.TryHandle(obj);
                    else
                        handlers.RemoveAt(i);
                }
        }
    }
}