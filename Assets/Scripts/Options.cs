using System.Collections;
using System.Collections.Generic;
using Controls;
using UnityEngine;

public static class Options
{
    private const string ControlPrefix = "DefRoomControl";

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static string InputDevice
    {
        get { return PlayerPrefs.GetString("InputDevice"); }
        set { PlayerPrefs.SetString("InputDevice", value); }
    }

    public static string OutputDevice
    {
        get { return PlayerPrefs.GetString("OutputDevice"); }
        set { PlayerPrefs.SetString("OutputDevice", value); }
    }

    public static string LastPreset
    {
        get { return PlayerPrefs.GetString("LastPreset"); }
        set { PlayerPrefs.SetString("LastPreset", value); }
    }

    public static string LastFile
    {
        get { return PlayerPrefs.GetString("LastFile"); }
        set { PlayerPrefs.SetString("LastFile", value); }
    }

    public static string GetControl(ControllableAction control)
    {
        return PlayerPrefs.GetString(ControlPrefix + control);
    }

    public static void SetControl(ControllableAction control, string value)
    {
        PlayerPrefs.SetString(ControlPrefix + control, value);
    }
}
