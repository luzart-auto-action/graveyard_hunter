using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraveyardHunter.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type].Remove(handler);
        }

        public static void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type)) return;

            // Copy to avoid modification during iteration
            var list = new List<Delegate>(_handlers[type]);
            foreach (var handler in list)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error handling {type.Name}: {e}");
                }
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
        }
    }
}
