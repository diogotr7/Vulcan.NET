using System.Collections.Generic;

namespace Vulcan.NET
{
    internal static class FullSizeKeyMapping
    {
        internal static Dictionary<Key, int> Mapping { get; } = new Dictionary<Key, int>()
        {
            [Key.ESC] = 0,
            [Key.TILDE] = 1,
            [Key.TAB] = 2,
            [Key.CAPS_LOCK] = 3,
            [Key.LEFT_SHIFT] = 4,
            [Key.LEFT_CONTROL] = 5,
            //Column 2
            [Key.ONE] = 6,
            [Key.Q] = 7,
            [Key.A] = 8,
            [Key.ISO_BACKSLASH] = 9,//ISO only
            [Key.LEFT_WINDOWS] = 10,
            //Column 3
            [Key.F1] = 11,
            [Key.TWO] = 12,
            [Key.W] = 13,
            [Key.S] = 14,
            [Key.Z] = 15,
            [Key.LEFT_ALT] = 16,
            //Column 4
            [Key.F2] = 17,
            [Key.THREE] = 18,
            [Key.E] = 19,
            [Key.D] = 20,
            [Key.X] = 21,
            //Column 5
            [Key.F3] = 23,
            [Key.FOUR] = 24,
            [Key.R] = 25,
            [Key.F] = 26,
            [Key.C] = 27,
            //Column 6
            [Key.F4] = 28,
            [Key.FIVE] = 29,
            [Key.T] = 30,
            [Key.G] = 31,
            [Key.V] = 32,
            //Column 7
            [Key.SIX] = 33,
            [Key.Y] = 34,
            [Key.H] = 35,
            [Key.B] = 36,
            [Key.SPACE] = 37,
            //Column 8
            [Key.F5] = 48,
            [Key.SEVEN] = 49,
            [Key.U] = 50,
            [Key.J] = 51,
            [Key.N] = 52,
            //Column 9
            [Key.F6] = 53,
            [Key.EIGHT] = 54,
            [Key.I] = 55,
            [Key.K] = 56,
            [Key.M] = 57,
            //Column 10
            [Key.F7] = 59,
            [Key.NINE] = 60,
            [Key.O] = 61,
            [Key.L] = 62,
            [Key.COMMA] = 63,
            //Column 11
            [Key.F8] = 65,
            [Key.ZERO] = 66,
            [Key.P] = 67,
            [Key.SEMICOLON] = 68,
            [Key.PERIOD] = 69,
            [Key.RIGHT_ALT] = 70,
            //Column 12
            [Key.MINUS] = 72,
            [Key.OPEN_BRACKET] = 73,
            [Key.APOSTROPHE] = 74,
            [Key.FORWARD_SLASH] = 75,
            [Key.FN_Key] = 76,
            //Column 13
            [Key.F9] = 78,
            [Key.EQUALS] = 79,
            [Key.CLOSE_BRACKET] = 80,
            [Key.BACKSLASH] = 81,//ANSI only
            [Key.RIGHT_SHIFT] = 82,
            [Key.APPLICATION_SELECT] = 83,
            //Column 14
            [Key.F10] = 84,
            [Key.F11] = 85,
            [Key.F12] = 86,
            [Key.BACKSPACE] = 87,
            [Key.ENTER] = 88,
            [Key.RIGHT_CONTROL] = 89,
            [Key.ISO_HASH] = 96,//ISO ONLY
            //Column 15
            [Key.PRINT_SCREEN] = 99,
            [Key.INSERT] = 100,
            [Key.DELETE] = 101,
            [Key.ARROW_LEFT] = 102,
            //Column 16
            [Key.SCROLL_LOCK] = 103,
            [Key.HOME] = 104,
            [Key.END] = 105,
            [Key.ARROW_UP] = 106,
            [Key.ARROW_DOWN] = 107,
            //Column 17
            [Key.PAUSE_BREAK] = 108,
            [Key.PAGE_UP] = 109,
            [Key.PAGE_DOWN] = 110,
            [Key.ARROW_RIGHT] = 111,
            //Column 18
            [Key.NUM_LOCK] = 113,
            [Key.NUM_SEVEN] = 114,
            [Key.NUM_FOUR] = 115,
            [Key.NUM_ONE] = 116,
            [Key.NUM_ZERO] = 117,
            //Column 19
            [Key.NUM_SLASH] = 119,
            [Key.NUM_EIGHT] = 120,
            [Key.NUM_FIVE] = 121,
            [Key.NUM_TWO] = 122,
            //Column 20
            [Key.NUM_ASTERISK] = 124,
            [Key.NUM_NINE] = 125,
            [Key.NUM_SIX] = 126,
            [Key.NUM_THREE] = 127,
            [Key.NUM_PERIOD] = 128,
            //Column 21
            [Key.NUM_MINUS] = 129,
            [Key.NUM_PLUS] = 130,
            [Key.NUM_ENTER] = 131
        };
    }
}
