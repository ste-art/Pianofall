using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Mono.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlayModes;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;


public class Loader : MonoBehaviour
{
    public static Loader Instance;

    public GameObject NetworkController;
    public MidiSequencer MidiSequencer;
    public GameObject PrerenderSequencer;
    public GameObject JustAudio;
    public AudioProcessor AudioProcessor;
    public GameObject Floor;
    public Material BackgroundMaterial;

    public Settings Settings;

    //private string _configName = "./defaults.conf";

    void Awake()
    {
        Globals.Loader = this;
    }

    void Start ()
    {
        try
        {
            Instance = this;
            NoteHelper.SerializeColorConfigs();
            //_configName = Path.Combine(Globals.RootFolder, "defaults.conf");

            Settings = Globals.Settings ?? Settings.Generate();
            Globals.Settings = Settings;

            ApplySettings();

            if (!string.IsNullOrEmpty(Settings.BackgroundColor))
            {
                Color clr;
                if(ColorUtility.TryParseHtmlString(Settings.BackgroundColor, out clr))
                {
                    BackgroundMaterial.SetColor("_Tint", clr);
                    RenderSettings.skybox = BackgroundMaterial;
                }
            }
            if (!string.IsNullOrEmpty(Settings.FloorColor))
            {
                Color clr;
                if (ColorUtility.TryParseHtmlString(Settings.FloorColor, out clr))
                {
                    Floor.GetComponent<Renderer>().material.color = clr;
                }
            }
            Floor.transform.Rotate(Settings.FloorRotation.x, 0, 0);
            Floor.transform.Rotate(0, Settings.FloorRotation.y, 0);
            Floor.transform.Rotate(0, 0, Settings.FloorRotation.z);
            Floor.transform.position += Settings.FloorPosition;
            Settings.FloorScale.Scale(Floor.transform.localScale);
            Floor.transform.localScale = Settings.FloorScale;

            var colorConfig = Settings.ColorConfig;
            if (!File.Exists(colorConfig))
            {
                colorConfig = Path.Combine(FileSystemHelpers.ColorConfigs.FullName, colorConfig);
            }
            if (!File.Exists(colorConfig))
            {
                Debug.Log(colorConfig);
                throw new Exception("Color config does not exist.");
            }
            NoteHelper.LoadConfig(colorConfig);
            //throw new Exception("Test");
            Debug.Log(JsonConvert.SerializeObject(Settings, Formatting.Indented,new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            SendAnalitics();
        }
        catch (Exception e)
        {
            GameObject.Find("ErrorText").GetComponent<Text>().text = e.Message;
            throw;
        }
    }

    private void SendAnalitics()
    {
        var baseDictionary = new Dictionary<string,object>
        {
            {"ColorConfig", Settings.ColorConfig.Clamp(50) },
            {"SoundFont", Settings.Soundfont.Clamp(20) },
            {"BuildInAudio", Settings.UseBuildInAduio },
            {"MidiInput", Settings.UseMidiInput },
            {"MidiOutput", Settings.UseMidiOutput },
            {"BlockLimit", Settings.Blocks }
        };

        if (string.IsNullOrEmpty(Settings.MidiPath))
        {
            Analytics.CustomEvent("FreePlay", baseDictionary);
            return;
        }
        baseDictionary.Add("Mode", Settings.Mode.ToString());
        baseDictionary.Add("TrackName", Path.GetFileNameWithoutExtension(Settings.MidiPath).Clamp(150));
        Analytics.CustomEvent("FilePlay", baseDictionary);
    }

    private void ApplySettings()
    {
        GameObject component = null;

        //if (string.IsNullOrEmpty(Settings.MidiPath))
        //{
        //    throw new ArgumentException("Midi file is not specified.");
        //}

        switch (Settings.Mode)
        {
            default:
            case Modes.Realtime:
                {
                    component = MidiSequencer.gameObject;
                    MidiSequencer.MidiPath = Settings.MidiPath;
                    MidiSequencer.NoDoubles = Settings.NoDoubles;
                    AudioProcessor.Piano = Settings.Soundfont;
                    AudioProcessor.Volume = Settings.Volume / 100f;
                    AudioProcessor.gameObject.SetActive(true);
                    break;
                }
            case Modes.Prerender:
                {
                    if (string.IsNullOrEmpty(Settings.OutPath))
                        throw new ArgumentException("Output folder file is not specified.");
                    component = PrerenderSequencer;
                    var subComp = component.GetComponent<PrerenderSequencer>();
                    subComp.MidiPath = Settings.MidiPath;
                    global::PrerenderSequencer.OutDir = Settings.OutPath;
                    subComp.NoDoubles = Settings.NoDoubles;
                    subComp.Piano = Settings.Soundfont;
                    subComp.Volume = Settings.Volume / 100.0;
                    break;
                }
            case Modes.Audio:
                {
                    if (string.IsNullOrEmpty(Settings.OutPath))
                        throw new ArgumentException("Output folder file is not specified.");
                    component = JustAudio;
                    var subComp = component.GetComponent<JustAudio>();
                    subComp.MidiPath = Settings.MidiPath;
                    subComp.OutDir = Settings.OutPath;
                    subComp.Piano = Settings.Soundfont;
                    subComp.Volume = Settings.Volume / 100.0;
                    break;
                }
        }
        GameObject.Find("CubeController").GetComponent<CubeController>().SetMaxNotes(Settings.Blocks);
        component.SetActive(true);
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum Modes
{
    //Network,
    Realtime,
    Prerender,
    Audio
}
