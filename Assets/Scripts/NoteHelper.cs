using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sanford.Multimedia.Midi;
using Random = System.Random;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NoteHelper
{
    private static IColorSelector _colorConfig = new BwColor();
    public static Color GetColor(ChannelMessage message)
    {
        return _colorConfig.GetColor(message);
    }

    public static bool IsKeyBlack(int id)
    {
        var n = id%12;
        return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
    }

    public static void SerializeColorConfigs()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof (IColorSelector).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        foreach (var type in types)
        {
            var path = Path.Combine(FileSystemHelpers.ColorConfigs.FullName, type.Name + ".conf");
            if (File.Exists(path))
            {
                continue;
            }
            var obj = Activator.CreateInstance(type);
            var text = JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
            File.WriteAllText(path, text);
        }
    }

    public static void LoadConfig(string colorConfigPath)
    {
        var text = File.ReadAllText(colorConfigPath);
        var obj = (JObject) JsonConvert.DeserializeObject(text);
        var typeName = (string) obj["Type"];
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .First(p => typeof (IColorSelector).IsAssignableFrom(p) && !p.IsInterface && p.Name.EndsWith(typeName));
        _colorConfig = (IColorSelector) obj.ToObject(type);
        _colorConfig.Initialize();
    }
}

public interface IColorSelector
{
    Color GetColor(ChannelMessage message);
    void Initialize();
}

public abstract class BaseSelector : IColorSelector
{
    [JsonProperty(Order = -2)] public string Type;

    protected BaseSelector()
    {
        Type = GetType().Name;
    }

    public abstract Color GetColor(ChannelMessage message);
    public abstract void Initialize();
}

public class BwColor : BaseSelector
{
    public string White = "#FFFFFF";
    public string Black = "#000000";

    [JsonIgnore] private Color _black = Color.black;
    [JsonIgnore] private Color _white = Color.white;

    public override void Initialize()
    {

        Color clr;
        _white = ColorUtility.TryParseHtmlString(White, out clr) ? clr : Color.white;
        _black = ColorUtility.TryParseHtmlString(Black, out clr) ? clr : Color.black;
    }

    public override Color GetColor(ChannelMessage message)
    {
        return NoteHelper.IsKeyBlack(message.Data1) ? _black : _white;
    }
}

public class RainbowColor : BaseSelector
{
    public int StartNote = 21;
    public int EndNote = 128;
    public float StartHue = 0f;
    public float EndHue = 0.9386f;
    public float Saturation = 1f;
    public float Value = 1f;

    public override Color GetColor(ChannelMessage message)
    {
        if (message.Data1 >= StartNote)
        {
            if ((message.Data1 <= EndNote))
            {
                return ColorHelper.FromHSV(
                        Mathf.Lerp(StartHue, EndHue, Mathf.InverseLerp(StartNote, EndNote, message.Data1)), Saturation,
                        Value);
            }
            else
            {
                return ColorHelper.FromHSV(EndHue, Saturation, Value);
            }
        }
        else
        {
            return ColorHelper.FromHSV(StartHue, Saturation, Value);
        }
    }

    public override void Initialize()
    {

    }
}

public class RandomHue : BaseSelector
{
    public float SaturationWhite = 0.8f;
    public float SaturationBlack = 1f;
    public float ValueBlack = 0.15f;
    public float ValueWhite = 1f;

    protected readonly Random Rnd = new Random();

    public override Color GetColor(ChannelMessage message)
    {
        var black = NoteHelper.IsKeyBlack(message.Data1);
        return ColorHelper.FromHSV(
            (float) Rnd.NextDouble(),
            black ? SaturationBlack : SaturationWhite,
            black ? ValueBlack : ValueWhite);
    }

    public override void Initialize()
    {

    }
}

public class TrackChannelRandomHue : RandomHue
{
    [JsonIgnore] private readonly Dictionary<int, Color?[,]> _colors = new Dictionary<int, Color?[,]>();

    public override Color GetColor(ChannelMessage message)
    {
        var track = message.TrackIndex;
        //Debug.Log(track);
        var channel = message.MidiChannel;

        if (!_colors.ContainsKey(track))
        {
            _colors.Add(track, new Color?[16, 2]);
        }
        var tracked = _colors[track];
        var black = NoteHelper.IsKeyBlack(message.Data1) ? 1 : 0;
        if (tracked[channel, black] == null)
        {
            var hue = (float) Rnd.NextDouble();
            tracked[channel, 0] = ColorHelper.FromHSV(hue, SaturationWhite, ValueWhite);
            tracked[channel, 1] = ColorHelper.FromHSV(hue, SaturationBlack, ValueBlack);
        }
        return (Color) tracked[channel, black];
    }
}

public class ChannelRGB : BaseSelector
{
    [JsonIgnore] public Color[,] Settings;

    public override void Initialize()
    {
        Settings = new Color[16, 2];
        for (var i = 0; i < ChannelColors.Length; i++)
        {
            Color clr;
            Settings[i, 0] = ColorUtility.TryParseHtmlString(ChannelColors[i].White, out clr) ? clr : Color.white;
            Settings[i, 1] = ColorUtility.TryParseHtmlString(ChannelColors[i].Black, out clr) ? clr : Color.black;
        }
        for (var i = ChannelColors.Length; i < 16; i++)
        {
            Settings[i, 0] = Color.white;
            Settings[i, 1] = Color.black;
        }
    }

    public override Color GetColor(ChannelMessage message)
    {
        return Settings[message.MidiChannel, NoteHelper.IsKeyBlack(message.Data1) ? 1 : 0];
    }

    public ColorSetString[] ChannelColors =
    {
        new ColorSetString("#32FF47", "#002603"),
        new ColorSetString("#3247FF", "#000326"),
        new ColorSetString("#FEFF32", "#3F3F00"),
        new ColorSetString("#FF8832", "#3F1A00"),
        new ColorSetString("#32FF47", "#002603"),
        new ColorSetString("#3247FF", "#000326"),
        new ColorSetString("#FEFF32", "#3F3F00"),
        new ColorSetString("#FF8832", "#3F1A00"),
        new ColorSetString("#32FF47", "#002603"),
        new ColorSetString("#3247FF", "#000326"),
        new ColorSetString("#FEFF32", "#3F3F00"),
        new ColorSetString("#FF8832", "#3F1A00"),
        new ColorSetString("#32FF47", "#002603"),
        new ColorSetString("#3247FF", "#000326"),
        new ColorSetString("#FEFF32", "#3F3F00"),
        new ColorSetString("#FF8832", "#3F1A00"),
    };
}

public class ColorSetString
{
    public string White;
    public string Black;

    public ColorSetString(string white, string black)
    {
        Black = black;
        White = white;
    }

    public ColorSetString()
    {
    }
}


