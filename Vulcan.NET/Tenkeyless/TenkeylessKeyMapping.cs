using System.Collections.Generic;

namespace Vulcan.NET
{
    internal static class TenkeylessKeyMapping
    {
        internal static Dictionary<Key, int> Mapping { get; } = new Dictionary<Key, int>
        {
            [Key.ESC] = 2,
            [Key.TILDE] = 3,
            [Key.TAB] = 4,
            [Key.CAPS_LOCK] = 5,
            [Key.LEFT_SHIFT] = 0,
            [Key.LEFT_CONTROL] = 1,
            //Column 2
            [Key.ONE] = 8,
            [Key.Q] = 9,
            [Key.A] = 10,
            [Key.ISO_BACKSLASH] = 6,//ISO only
            [Key.LEFT_WINDOWS] = 7,
            //Column 3
            [Key.F1] = 13,
            [Key.TWO] = 14,
            [Key.W] = 15,
            [Key.S] = 16,
            [Key.Z] = 11,
            [Key.LEFT_ALT] = 12,
            //Column 4
            [Key.F2] = 20,
            [Key.THREE] = 21,
            [Key.E] = 22,
            [Key.D] = 23,
            [Key.X] = 17,
            //Column 5
            [Key.F3] = 25,
            [Key.FOUR] = 26,
            [Key.R] = 27,
            [Key.F] = 28,
            [Key.C] = 24,
            //Column 6
            [Key.F4] = 30,
            [Key.FIVE] = 31,
            [Key.T] = 32,
            [Key.G] = 33,
            [Key.V] = 29,
            //Column 7
            [Key.SIX] = 36,
            [Key.Y] = 37,
            [Key.H] = 38,
            [Key.B] = 34,
            [Key.SPACE] = 35,
            //Column 8
            [Key.F5] = 40,
            [Key.SEVEN] = 41,
            [Key.U] = 42,
            [Key.J] = 43,
            [Key.N] = 39,
            //Column 9
            [Key.F6] = 47,
            [Key.EIGHT] = 48,
            [Key.I] = 49,
            [Key.K] = 50,
            [Key.M] = 44,
            //Column 10
            [Key.F7] = 53,
            [Key.NINE] = 54,
            [Key.O] = 55,
            [Key.L] = 56,
            [Key.COMMA] = 51,
            //Column 11
            [Key.F8] = 59,
            [Key.ZERO] = 60,
            [Key.P] = 61,
            [Key.SEMICOLON] = 62,
            [Key.PERIOD] = 57,
            [Key.RIGHT_ALT] = 58,
            //Column 12
            [Key.F9] = 65,
            [Key.MINUS] = 66,
            [Key.OPEN_BRACKET] = 67,
            [Key.APOSTROPHE] = 68,
            [Key.FORWARD_SLASH] = 63,
            [Key.FN_Key] = 64,
            //Column 13
            [Key.F10] = 71,
            [Key.EQUALS] = 72,
            [Key.CLOSE_BRACKET] = 73,
            [Key.ISO_HASH] = 74,//ISO ONLY
            [Key.RIGHT_SHIFT] = 75,
            [Key.APPLICATION_SELECT] = 70,
            //Column 14
            [Key.F11] = 77,
            //Column 15
            [Key.F12] = 79,
            [Key.BACKSPACE] = 80,
            [Key.BACKSLASH] = 81,//ANSI only
            [Key.ENTER] = 82,
            [Key.RIGHT_CONTROL] = 76,
            //Column 16
            [Key.MUTE] = 92,
            [Key.INSERT] = 84,
            [Key.DELETE] = 85,
            [Key.ARROW_LEFT] = 86,
            //Column 17
            [Key.HOME] = 88,
            [Key.END] = 89,
            [Key.ARROW_UP] = 90,
            [Key.ARROW_DOWN] = 91,
            //Column 18
            [Key.PAGE_UP] = 93,
            [Key.PAGE_DOWN] = 94,
            [Key.ARROW_RIGHT] = 95,
        };
    }
}
