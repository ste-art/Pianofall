using System;
using UnityEngine;
using System.Collections;

public static class ColorHelper
{
    private const float Tolerance = 0.001f;
    public static Color FromHSV(float h, float s, float v)
    {
        if (Math.Abs(s) < Tolerance)
            return new Color(v, v, v);
        if (Math.Abs(v) < Tolerance)
            return Color.black;

        Color col = Color.black;
        float hVal = h*6f;
        int sel = Mathf.FloorToInt(hVal);
        float mod = hVal - sel;
        float v1 = v*(1f - s);
        float v2 = v*(1f - s*mod);
        float v3 = v*(1f - s*(1f - mod));
        switch (sel + 1)
        {
            case 0:
                col.r = v;
                col.g = v1;
                col.b = v2;
                break;
            case 1:
                col.r = v;
                col.g = v3;
                col.b = v1;
                break;
            case 2:
                col.r = v2;
                col.g = v;
                col.b = v1;
                break;
            case 3:
                col.r = v1;
                col.g = v;
                col.b = v3;
                break;
            case 4:
                col.r = v1;
                col.g = v2;
                col.b = v;
                break;
            case 5:
                col.r = v3;
                col.g = v1;
                col.b = v;
                break;
            case 6:
                col.r = v;
                col.g = v1;
                col.b = v2;
                break;
            case 7:
                col.r = v;
                col.g = v3;
                col.b = v1;
                break;
        }
        col.r = Mathf.Clamp(col.r, 0f, 1f);
        col.g = Mathf.Clamp(col.g, 0f, 1f);
        col.b = Mathf.Clamp(col.b, 0f, 1f);
        return col;
    }
}
