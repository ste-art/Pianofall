using System.Collections;
using System.Collections.Generic;
using PlayModes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MicroMenu : MonoBehaviour {

    void Awake()
    {
        Globals.MicroMenu = this;
    }

    private Transform _background;
    private Vector3 _position;
    private bool _visible = false;

    void Start()
    {
        _background = transform.Find("Background");
        _position = _background.position;
        _background.position = new Vector3(-100000, 0, 0);
        var play = transform.Find("Background/Play").GetComponent<Button>();
        var restart = transform.Find("Background/Restart").GetComponent<Button>();
        transform.Find("Background/MainMenu").GetComponent<Button>().onClick.AddListener(OnMainMenu);
        transform.Find("Background/Exit").GetComponent<Button>().onClick.AddListener(OnExit);

        play.onClick.AddListener(OnPlay);
        restart.onClick.AddListener(OnRestart);

        
        play.interactable = restart.interactable = Globals.Settings != null && !string.IsNullOrEmpty(Globals.Settings.MidiPath);
    }

    private void OnMainMenu()
    {
        SceneManager.LoadScene("MenuDebug", LoadSceneMode.Single);
    }

    private void OnExit()
    {
        MainMenu.Exit();
    }

    private void OnRestart()
    {
        var seuqencer = Globals.Loader.MidiSequencer.GetComponent<MidiSequencer>();
        if (seuqencer == null) return;
        seuqencer.Sequencer.Start();
    }

    public void OnPlay()
    {
        var seuqencer = Globals.Loader.MidiSequencer;
        if (seuqencer == null) return;
        if (seuqencer.Sequencer.Playing)
        {
            seuqencer.Sequencer.Stop();
        }
        else
        {
            seuqencer.Sequencer.Continue();
        }
    }

    void Update ()
    {
        if (Input.GetKeyDown("escape"))
        {
            _visible = !_visible;
            _background.position = _visible ? _position : new Vector3(-100000, 0, 0);
        }
    }
}
