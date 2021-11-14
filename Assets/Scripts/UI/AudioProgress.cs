using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioProgress : MonoBehaviour
{

    private static AudioProgress _instance;
    private static Text _text;

    void Awake()
    {
        _instance = this;
    }

    void Start () {
        gameObject.SetActive(false);
        _text = transform.Find("Panel/Text").GetComponent<Text>();
    }

    public static void Enable()
    {
        _instance.gameObject.SetActive(true);
    }

    public static void SetText(string text)
    {
        _text.text = text;
    }
}
