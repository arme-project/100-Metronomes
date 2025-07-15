//
// Minimal main-thread dispatcher.
//
// Usage:
//   UnityMainThreadDispatcher.Enqueue(() => { /* code that must run on main thread */ });
//
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
                _queue.Dequeue().Invoke();
        }
    }

    /// <summary>Add an action to execute on the next main-thread Update.</summary>
    public static void Enqueue(Action action)
    {
        if (action == null) return;
        lock (_queue) _queue.Enqueue(action);
    }
}
