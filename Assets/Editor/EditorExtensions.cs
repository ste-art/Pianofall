#if UNITY_EDITOR
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public static class EditorExtensions
{
    [MenuItem("Tools/Build x86")]
    public static void Build_x86()
    {
        Build("Build_x86",BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build x64")]
    public static void Build_x64()
    {
        Build("Build_x64", BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/Build_Linux")]
    public static void Build_Linux()
    {
        var folder = "Build_Linux";
        Build(folder, BuildTarget.StandaloneLinux64);
        var libMidiSoSource = Path.Combine(folder, "Pianofall_Data/Plugins/librtmidi.so");
        var libMidiSoDest = Path.Combine(folder, "Pianofall_Data/il2cpp_data/Resources/librtmidi.so");
        File.Copy(libMidiSoSource, libMidiSoDest, true);
    }

    [MenuItem("Tools/Build_Mac")]
    public static void Build_Mac()
    {
        Build("Build_Mac", BuildTarget.StandaloneOSX);
    }

    //[MenuItem("Tools/Build All/Start")]
    public static void BuildAll()
    {
        Build_x86();
        Build_x64();
        Build_Linux();
        Build_Mac();
    }

    [MenuItem("Tools/Build All/Deploy")]
    public static void BuildAndDeploy()
    {
        Deploy(false);
    }

    [MenuItem("Tools/Build All/Silent deploy")]
    public static void BuildAndDeploySilent()
    {
        Deploy(true);
    }

    private static void Deploy(bool silent)
    {
        const string buildPrefix = "Build_";
        var builds = new[] {"x86", "x64", /*"Linux", "Mac"*/};

        var directories = builds.Select(b => buildPrefix + b);
        foreach (var directory in directories)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        Build_x86();
        Build_x64();
        //Build_Linux();
        //Build_Mac();

        var deploy = "!Deploy";
        if (Directory.Exists(deploy))
        {
            Directory.Delete(deploy, true);
        }
        Directory.CreateDirectory(deploy);

        var version = MainMenu.GetVersionText();
        foreach (var build in builds)
        {
            var directory = buildPrefix + build;
            var archive = Path.Combine(deploy, string.Format("Pianofall_{0}_{1}.zip", version, build));
            Zip(directory, archive, silent);
            if (silent)
            {
                Debug.LogFormat("Deployed: {0}", build);
            }
        }

        Process.Start(deploy);
    }

    private static void Zip(string directory, string archive, bool silent)
    {
        var info = new ProcessStartInfo
        {
            FileName = ".\\Tools\\7z.exe",
            Arguments = string.Format("a -tzip \"{0}\" \".\\{1}\\*\"", archive, directory),
        };
        if (silent)
        {
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
        }
        Process.Start(info).WaitForExit();
    }


    public static void Build(string folder, BuildTarget target)
    {
        Debug.ClearDeveloperConsole();
        Debug.Log(string.Format("Building {0}", folder));
        folder = "./" + folder;
        var scenes = new[] {"Assets/MenuDebug.unity", "Assets/1.unity"};
        scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, target == BuildTarget.StandaloneLinux64 ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
        BuildPipeline.BuildPlayer(scenes, folder + "/Pianofall.exe", target, BuildOptions.None);

        CopyFiles(folder, Directory.GetFiles("./AdditionalFiles"));
    }

    private static void CopyFiles(string directory, params string[] files)
    {
        foreach (var file in files)
        {
            File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);
        }
    }
}
#endif