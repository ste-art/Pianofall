using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Keyboard
{
    public static KeyCode[] Keys;
    static Keyboard()
    {
        var keys = new List<KeyCode>();
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            var code = (int) key;
            if (code >= 8 && code <= 319)
            {
                keys.Add(key);
            }
        }
        Keys = keys.ToArray();
    }

    public static KeyCode GetKeyDown()
    {
        return Keys.FirstOrDefault(Input.GetKeyDown);
    }

    public static KeyCode[] GetKeyCodes()
    {
        return Keys.Where(Input.GetKeyDown).ToArray();
    }
}
