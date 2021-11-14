using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Mono.Options;
using UnityEngine;

[Serializable]
public class Settings
{
    public const string DefaultName = "Default";


    public Modes Mode = Modes.Realtime;
    public string MidiPath;
    public int NoDoubles = 0;
    public string OutPath = "Out";
    public int Blocks = 0;
    public string Soundfont = "piano1";
    public int Volume = 75;
    public string BackgroundColor = "#DCFEFE";
    public string FloorColor = "#101010FF";
    public Vector3 FloorPosition = Vector3.zero;
    public Vector3 FloorRotation = Vector3.zero;
    public Vector3 FloorScale = Vector3.one;
    public string ColorConfig = "BwColor.conf";
    public bool UseBuildInAduio = true;
    public bool UseMidiInput = true;
    public bool UseMidiOutput = false;
    public int? InstrumentOverride = null;

    public static Settings Deseriaize(string name)
    {
        var configName = GetFullPath(name);
        var s = new XmlSerializer(typeof (Settings));

        if (!File.Exists(configName))
        {
            return Serialize(name);
        }

        using (var f = File.OpenText(configName))
        {
            var settings = (Settings) s.Deserialize(f);
            settings.FixOutPath();
            return settings;
        }
    }

    public static Settings Serialize(string name)
    {
        var settings = new Settings();
        settings.Save(name);
        return settings;
    }

    public void Save(string name)
    {
        var configName = GetFullPath(name);
        var serializer = new XmlSerializer(typeof(Settings));

        using (var stream = File.CreateText(configName))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static void Delete(string name)
    {
        try
        {
            var configName = GetFullPath(name);
            File.Delete(configName);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private static string GetFullPath(string name)
    {
        return Path.Combine(FileSystemHelpers.Presets.FullName, name + ".conf");
    }

    public void ParseArguments()
    {
        bool showHelp = false;
        string newMidi = null;
        var set = new OptionSet()
        {
            {"m|mode=", "Pianofall mode.", v => { Mode = (Modes) Enum.Parse(typeof (Modes), v, true); }},
            {"f|file=", "Midi file path.", v => newMidi = v},
            {"d|doubles=", "Doubles reuction mode.", (int v) => NoDoubles = v},
            {"o|out|output=", "Output folder.", v => OutPath = v},
            {"b|blocks=", "Blocks limit.", (int v) => Blocks = v},
            {"s|soundfont=", "Soundfont.", v => Soundfont = v},
            {"h|help", "Show help message", v => showHelp = v!=null},
            {"v|volume=", "Audio volume.",(int v) => Volume = v},
            {"c|color=","Color config", v => ColorConfig = v},
            {"bgc|bgcolor=", "Background color", v => BackgroundColor = v},
            {"fc|floorcolor=", "Floor color", v => FloorColor = v},
        };
#if UNITY_EDITOR
        var extra = new List<string>();
#else
        var extra = set.Parse(Environment.GetCommandLineArgs().Skip(1));
#endif

        MidiPath = newMidi ?? extra.FirstOrDefault() ?? MidiPath;
        if(MidiPath == "-projectpath")
        {
            MidiPath = null;
        }

        if (showHelp)
        {
            set.WriteOptionDescriptions(Console.Out);
        }

        if (Blocks == 0)
        {
            Blocks = Mode == Modes.Realtime ? 1000 : 10000;
        }
    }

    private void FixOutPath()
    {
        if (!Path.IsPathRooted(OutPath))
        {
            OutPath = Path.Combine(Globals.RootFolder, OutPath);
        }
    }

    private static bool _argsRequired = true;

    public static Settings Generate()
    {
        var settings = Serialize("default");
        if(_argsRequired)
        {
            settings.ParseArguments();
            _argsRequired = false;
        }
        settings.FixOutPath();
        return settings;
    }
}
