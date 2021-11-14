using UnityEngine;
using System.Collections;
using System;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));

                if (_instance == null)
                {
                    Debug.LogError("An instance of " + typeof(T) +
                       " is needed in the scene, but there is none.");
                }
            }

            return _instance;
        }
    }

    public static Lazy<T> Get()
    {
        return new Lazy<T>(() => FindObjectOfType(typeof(T)) as T);
    }
}
