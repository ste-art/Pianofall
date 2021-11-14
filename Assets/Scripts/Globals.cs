using System.Collections;
using System.Collections.Generic;
using System.IO;
using UI;
using UnityEngine;

public static class Globals
{
    public static Settings Settings;

    public static string RootFolder
    {
        get { return FileSystemHelpers.RootFolder; }
    }

    public static Loader Loader;

    public static MenuController StartMenu;
    public static OptionsMenu OptionsMenu;
    public static MicroMenu MicroMenu;
}
