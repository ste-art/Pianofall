using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public Text Version;

    private Button _start;
    private Button _exit;
    private Button _options;


    void Start ()
    {
        _start = transform.Find("Background/MainPanel/StartButton").GetComponent<Button>();
        _start.onClick.AddListener(OnStart);
        _options = transform.Find("Background/MainPanel/OptionsButton").GetComponent<Button>();
        _options.onClick.AddListener(OnOptions);
        _exit = transform.Find("Background/MainPanel/ExitButton").GetComponent<Button>();
        _exit.onClick.AddListener(OnExit);
        //transform.Find("Image").GetComponent<Image>().color = RenderSettings.skybox.GetColor("_Tint");

        if (new System.Random().Next(50) == 0)
        {
            MakeRainbowParticles();
        }

        var version = GetVersionText();
        Version.text = version;
        Analytics.CustomEvent("Launch", new Dictionary<string, object> {{"Version", version } });
    }

    public static string GetVersionText()
    {
        var version = typeof (MainMenu).Assembly.GetName().Version;
        var versionText = string.Format("v{0}.{1}", version.Major, version.Minor);
        if (version.Build != 0)
        {
            versionText += "." + version.Build;
        }
        return versionText;
    }

    private void MakeRainbowParticles()
    {
        var colorKeys = Enumerable.Range(0, 8).Select(i =>
            new GradientColorKey(
                ColorHelper.FromHSV(Mathf.Lerp(0, 1, Mathf.InverseLerp(0, 8, i)), 1, 1),
                i/8f)).ToArray();
        var mainParticle = ParticleSystem.main;
        mainParticle.startColor = new ParticleSystem.MinMaxGradient(new Gradient()
        {
            colorKeys = colorKeys,
            mode = GradientMode.Blend
        }) {mode = ParticleSystemGradientMode.RandomColor};
        ParticleSystem.Simulate(0);
        ParticleSystem.Play();
    }

    private void OnOptions()
    {
        Globals.OptionsMenu.gameObject.SetActive(true);
    }

    private void OnExit()
    {
        Exit();
    }

    private void OnStart()
    {
        Globals.StartMenu.gameObject.SetActive(true);
    }

    public static void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        //System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }
}
