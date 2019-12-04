using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : SingletonManager<T>
{
    private static T singleton;
    public static T Instance
    {
        get
        {
            return singleton;
        }
    }

    protected void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = (T)this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DestroySingleton()
    {
        singleton = null;
        Destroy(gameObject);
    }

    public void SingletonReset()
    {
        singleton = null;
    }
}
