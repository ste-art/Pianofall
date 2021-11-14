using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionDropDown : MonoBehaviour
{
    private Dropdown _dropdown;
    private Toggle _fullscreen;
    private Button _apply;

    private List<Resolution> _resolutions; 

    void Awake ()
    {
        _resolutions = Screen.resolutions
            .DistinctBy(ResolutionToText)
            .OrderBy(r => r.width)
            .ThenBy(r => r.height)
            .ToList();

        _dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        _fullscreen = transform.Find("Fullscreen").GetComponent<Toggle>();
        _apply = transform.Find("Apply").GetComponent<Button>();
        _apply.onClick.AddListener(ApplyResolution);

        _dropdown.options.Clear();
        _dropdown.AddOptions(_resolutions.Select(ResolutionToText).ToList());
    }

    void OnEnable()
    {
        _dropdown.value = GetCurrentResolutionIndex();
        _fullscreen.isOn = Screen.fullScreen;
    }

    private void ApplyResolution() 
    {
        var resolution = _resolutions[_dropdown.value];
        var fullscreen = _fullscreen.isOn;
        Screen.SetResolution(resolution.width, resolution.height, fullscreen);
    }

    private static string ResolutionToText(Resolution resolution)
    {
        return string.Format("{0}x{1}", resolution.width, resolution.height);
    }

    private int GetCurrentResolutionIndex()
    {
        var resolution = Screen.currentResolution;
        return Math.Max(0, _resolutions.FindIndex(r => r.width == resolution.width && r.height == resolution.height));
    }
}
