using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class ValueSlider : Initable
{
    public int Min
    {
        get { return ToInt(Slider.minValue); }
        set { Slider.minValue = value; }
    }

    public int Max
    {
        get { return ToInt(Slider.maxValue); }
        set { Slider.maxValue = value; }
    }

    public Func<int, int> Rounding;

    private int _value;
    public int Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            Text.text = value.ToString();
            Slider.value = value;
        }
    }

    public InputField Text;
    public Slider Slider;
    private bool _editable = true;

    void Awake()
    {
        if(Text == null)
        {
            Init();
        }
        // ReSharper disable once PossibleNullReferenceException
        Text.onValueChanged.AddListener(OnTextEdit);
        Slider.onValueChanged.AddListener(OnSliderMove);
    }

    public override void Init()
    {
        Text = transform.Find("InputField").GetComponent<InputField>();
        Slider = transform.Find("Slider").GetComponent<Slider>();
    }

    // ReSharper disable once InconsistentNaming
    public bool interactable
    {
        get { return Slider.interactable && Text.interactable; }
        set
        {
            Slider.interactable = value;
            Text.interactable = value;
        }
    }

    private void OnTextEdit(string value)
    {
        if (!_editable) return;
        int tempValue;
        if (!int.TryParse(value, out tempValue)) return;
        _value = tempValue;
        ExecuteUneditable(() => Slider.value = _value);
    }

    private void OnSliderMove(float value)
    {
        if (!_editable) return;
        _value = ToInt(value);
        _value = Rounding == null ? _value : Rounding(_value);
        ExecuteUneditable(() => { Text.text = _value.ToString(); });
    }

    private void ExecuteUneditable(Action action)
    {
        try
        {
            _editable = false;
            action();
        }
        finally
        {
            _editable = true;
        }
    }

    private int ToInt(float value)
    {
        return (int)Mathf.Round(value);
    }
}
