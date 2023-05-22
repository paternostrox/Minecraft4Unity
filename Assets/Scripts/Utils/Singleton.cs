using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T main;

    public static T Main
    {
        get
        {
            if (main != null) return main;

            main = (T)FindObjectOfType(typeof(T));

            if (main == null)
            {
                main = new GameObject(typeof(T).Name).AddComponent<T>();
            }

            //DontDestroyOnLoad(main);
            return main;
        }
    }

}