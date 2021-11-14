using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileSystemHelpers
{

    private static string _rootFolder;

    public static string RootFolder
    {
        get
        {
            if (_rootFolder == null)
            {
                InitRootFolder();
            }
            return _rootFolder;
        }
    }

    public static DirectoryInfo ColorConfigs
    {
        get { return GetDirectory("ColorConfigs"); }
    }

    public static DirectoryInfo Presets
    {
        get { return GetDirectory("Presets"); }
    }

    private static void InitRootFolder()
    {
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            _rootFolder = Application.persistentDataPath;
            //_rootFolder    =    Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
        }

        else //if    (Application.platform    ==    RuntimePlatform.WindowsPlayer    ||    Application.isEditor)
        {
            _rootFolder = Path.GetDirectoryName(Application.dataPath.Replace('/', '\\'));
        }
    }

    public static DirectoryInfo GetDirectory(string name)
    {
        var path = Path.Combine(RootFolder, name);
        var info = new DirectoryInfo(path);
        if (!info.Exists)
        {
            info.Create();
        }
        return info;
    }
}
