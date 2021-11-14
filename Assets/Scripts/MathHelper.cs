using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static int Round(int value, int mod)
    {
        return (int) Math.Round(value/(double) mod, 0) *mod;
    }
}
