using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskExecutor : MonoBehaviour
{
    private readonly Queue<Action> _actionQueue = new Queue<Action>();

    void Update ()
    {
        lock (_actionQueue)
        {
            var any = false;
            foreach (var action in _actionQueue)
            {
                any = true;
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if(any)
            {
                _actionQueue.Clear();
            }
        }
	}

    public void Enqueue(Action action)
    {
        lock (_actionQueue)
        {
            _actionQueue.Enqueue(action);
        }
    }
}
