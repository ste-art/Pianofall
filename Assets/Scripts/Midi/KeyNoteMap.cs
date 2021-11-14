using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midi
{
    public static class KeyNoteMap
    {
        public static Dictionary<KeyCode, int> Map = new Dictionary<KeyCode, int>
        {
            {KeyCode.A, 44},
            {KeyCode.Z, 45},
            {KeyCode.S, 46},
            {KeyCode.X, 47},
            {KeyCode.C, 48},
            {KeyCode.F, 49},
            {KeyCode.V, 50},
            {KeyCode.G, 51},
            {KeyCode.B, 52},
            {KeyCode.N, 53},
            {KeyCode.J, 54},
            {KeyCode.M, 55},
            {KeyCode.K, 56},
            {KeyCode.Comma, 57},
            {KeyCode.L, 58},
            {KeyCode.Period, 59},
            {KeyCode.Slash, 60},
            {KeyCode.Quote, 61 },
            {KeyCode.Alpha1, 56 },
            {KeyCode.Q, 57 },
            {KeyCode.Alpha2, 58 },
            {KeyCode.W,59 },
            {KeyCode.E, 60 },
            {KeyCode.Alpha4, 61 },
            {KeyCode.R,62 },
            {KeyCode.Alpha5, 63 },
            {KeyCode.T, 64 },
            {KeyCode.Y, 65 },
            {KeyCode.Alpha7, 66 },
            {KeyCode.U, 67 },
            {KeyCode.Alpha8, 68 },
            {KeyCode.I, 69 },
            {KeyCode.Alpha9, 70 },
            {KeyCode.O, 71 },
            {KeyCode.P, 72 },
            {KeyCode.Minus, 73 },
            {KeyCode.LeftBracket, 74 },
            {KeyCode.Equals, 75 },
            {KeyCode.RightBracket, 76 }
        };

        public static KeyCode[] GetKeysDown()
        {
            return Map.Keys.Where(Input.GetKeyDown).ToArray();
        }

        public static KeyCode[] GetKeysUp()
        {
            return Map.Keys.Where(Input.GetKeyUp).ToArray();
        }
    }
}
