using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports.Encodings;

namespace Vulcan.NET
{
    public static class VulcanKeyboard
    {
        private const int MaxTries = 100;
        private const int VendorId = 0x1E7D;
        private const uint LedUsagePage = 0x0001;
        private const uint LedUsage = 0x0000;
        private static readonly int[] ProductIds = new int[] { 0x307A, 0x3098 };
        private static readonly byte[] ColorPacketHeader = new byte[5] { 0x00, 0xa1, 0x01, 0x01, 0xb4 };

        private static HidDevice _ledDevice;
        private static HidStream _ledStream;
        private static HidDevice _ctrlDevice;
        private static HidStream _ctrlStream;
        private static readonly byte[] _keyColors = new byte[444];//64 * 6 + 60

        public static bool IsConnected { get; private set; }

        public static bool Initialize()
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId)
                        .Where(d => ProductIds.Any(id => id == d.ProductID));

            if (!devices.Any())
                return false;
            

            try
            {
                _ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);
                _ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);

                if ((_ctrlDevice?.TryOpen(out _ctrlStream) ?? false) && (_ledDevice?.TryOpen(out _ledStream) ?? false))
                {
                    if (SendCtrlInitSequence())
                        return true;

                }
                else
                {
                    _ctrlStream?.Close();
                    _ledStream?.Close();
                }
            }
            catch
            {
            }

            return false;
        }

        public static void SetColor(Color clr)
        {
            foreach (Key key in (Key[])Enum.GetValues(typeof(Key)))
                SetKeyColor(key, clr);           
        }

        public static void SetColors(Dictionary<Key, Color> keyColors)
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public static void SetKeyColor(Key key, Color clr)
        {
            int offset = ((int)key / 12 * 36) + ((int)key % 12);
            _keyColors[offset + 0] = clr.R;
            _keyColors[offset + 12] = clr.G;
            _keyColors[offset + 24] = clr.B;
        }

        public static bool Update() => WriteColorBuffer();

        public static void Disconnect()
        {
            _ctrlStream?.Close();
            _ledStream?.Close();
            IsConnected = false;
        }

        private static bool WriteColorBuffer()
        {
            //structure of the data: 
            //header *5
            //data *60

            //0x00 * 1
            //data *64

            //0x00 * 1
            //data *64

            //0x00 * 1
            //data *64

            //0x00 * 1
            //data *64

            //0x00 * 1
            //data *64

            //0x00 * 1
            //data *64

            byte[] packet = new byte[65];

            ColorPacketHeader.CopyTo(packet, 0);//header at the beginning of the first packet
            Array.Copy(_keyColors, 0,
                        packet, ColorPacketHeader.Length,
                        60);//copy the first 60 bytes of color data to the packet
                            //so 60 data + 5 header fits in a packet
            try
            {
                _ledStream.Write(packet);

                for (int i = 1; i <= 6; i++)//each chunk consists of the byte 0x00 and 64 bytes of data after that
                {
                    packet[0] = 0x00;
                    Array.Copy(_keyColors, (i * 64) - 4,//each packet holds 64 except for the first one, hence we subtract 4
                                packet, 1,
                                64);

                    _ledStream.Write(packet);
                }

                return true;
            }
            catch
            {
                Disconnect();
                return false;
            }
        }

        private static bool SendCtrlInitSequence()
        {
            var result =
                GetCtrlReport(0x0f) &&
                SetCtrlReport(CtrlReports._0x15) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x05) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x07) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x0a) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x0b) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x06) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x09) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x0d) &&
                WaitCtrlDevice() &&
                SetCtrlReport(CtrlReports._0x13) &&
                WaitCtrlDevice();

            _ctrlStream?.Close();

            return result;
        }

        private static bool GetCtrlReport(byte report_id)
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            var buf = new byte[size];
            buf[0] = report_id;
            try
            {
                _ctrlStream.GetFeature(buf);
                return true;
            }
            catch(Exception e)
            {
                //Console.WriteLine("Exception in getctrlreport: " + e.Message);
                return false;
            }
        }

        private static bool SetCtrlReport(byte[] reportBuffer)
        {
            try
            {
                _ctrlStream.SetFeature(reportBuffer);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool WaitCtrlDevice()
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            byte[] buf = new byte[size];
            buf[0] = 0x04;
            for (int i = 0; i < MaxTries; i++)
            {
                Thread.Sleep(150);
                try
                {
                    _ctrlStream.GetFeature(buf);
                    if (buf[1] == 0x01)
                        return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private static HidDevice GetFromUsages(IEnumerable<HidDevice> devices, uint usagePage, uint usage)
        {
            foreach (var dev in devices)
            {
                try
                {
                    var raw = dev.GetRawReportDescriptor();
                    var usages = EncodedItem.DecodeItems(raw, 0, raw.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);

                    if (usages.Any(g => g.ItemType == ItemType.Global && g.DataValue == usagePage))
                    {
                        if (usages.Any(l => l.ItemType == ItemType.Local && l.DataValue == usage))
                        {
                            Console.WriteLine("Found device with correct usages:" + dev);
                            return dev;
                        }
                    }
                }
                catch
                {
                    //failed to get the report descriptor, skip
                }
            }
            Console.WriteLine("Failed to get from usages");
            return null;
        }
    }

    internal static class CtrlReports
    {
        internal static readonly byte[] _0x15 = new byte[] {
                        0x15, 0x00, 0x01 };
        internal static readonly byte[] _0x05 = new byte[] {
                        0x05, 0x04, 0x00, 0x04 };
        internal static readonly byte[] _0x07 = new byte[] {
                        0x07 ,0x5f ,0x00 ,0x3a ,0x00 ,0x00 ,0x3b ,0x00 ,0x00 ,0x3c ,0x00 ,0x00 ,0x3d ,0x00 ,0x00 ,0x3e,
                        0x00 ,0x00 ,0x3f ,0x00 ,0x00 ,0x40 ,0x00 ,0x00 ,0x41 ,0x00 ,0x00 ,0x42 ,0x00 ,0x00 ,0x43 ,0x00,
                        0x00 ,0x44 ,0x00 ,0x00 ,0x45 ,0x00 ,0x00 ,0x46 ,0x00 ,0x00 ,0x47 ,0x00 ,0x00 ,0x48 ,0x00 ,0x00,
                        0xb3 ,0x00 ,0x00 ,0xb4 ,0x00 ,0x00 ,0xb5 ,0x00 ,0x00 ,0xb6 ,0x00 ,0x00 ,0xc2 ,0x00 ,0x00 ,0xc3,
                        0x00 ,0x00 ,0xc0 ,0x00 ,0x00 ,0xc1 ,0x00 ,0x00 ,0xce ,0x00 ,0x00 ,0xcf ,0x00 ,0x00 ,0xcc ,0x00,
                        0x00 ,0xcd ,0x00 ,0x00 ,0x46 ,0x00 ,0x00 ,0xfc ,0x00 ,0x00 ,0x48 ,0x00 ,0x00 ,0xcd ,0x0e };
        internal static readonly byte[] _0x0a = new byte[] {
                        0x0a, 0x08, 0x00, 0xff, 0xf1, 0x00, 0x02, 0x02 };
        internal static readonly byte[] _0x0b = new byte[] {
                        0x0b ,0x41 ,0x00 ,0x1e ,0x00 ,0x00 ,0x1f ,0x00 ,0x00 ,0x20 ,0x00 ,0x00 ,0x21 ,0x00 ,0x00 ,0x22,
                        0x00 ,0x00 ,0x14 ,0x00 ,0x00 ,0x1a ,0x00 ,0x00 ,0x08 ,0x00 ,0x00 ,0x15 ,0x00 ,0x00 ,0x17 ,0x00,
                        0x00 ,0x04 ,0x00 ,0x00 ,0x16 ,0x00 ,0x00 ,0x07 ,0x00 ,0x00 ,0x09 ,0x00 ,0x00 ,0x0a ,0x00 ,0x00,
                        0x1d ,0x00 ,0x00 ,0x1b ,0x00 ,0x00 ,0x06 ,0x00 ,0x00 ,0x19 ,0x00 ,0x00 ,0x05 ,0x00 ,0x00 ,0xde ,0x01};
        internal static readonly byte[] _0x06 = new byte[] {
                        0x06 ,0x85 ,0x00 ,0x3a ,0x29 ,0x35 ,0x1e ,0x2b ,0x39 ,0xe1 ,0xe0 ,0x3b ,0x1f ,0x14 ,0x1a ,0x04,
                        0x64 ,0x00 ,0x00 ,0x3d ,0x3c ,0x20 ,0x21 ,0x08 ,0x16 ,0x1d ,0xe2 ,0x3e ,0x23 ,0x22 ,0x15 ,0x07,
                        0x1b ,0x06 ,0x8b ,0x3f ,0x24 ,0x00 ,0x17 ,0x0a ,0x09 ,0x19 ,0x91 ,0x40 ,0x41 ,0x00 ,0x1c ,0x18,
                        0x0b ,0x05 ,0x2c ,0x42 ,0x26 ,0x25 ,0x0c ,0x0d ,0x0e ,0x10 ,0x11 ,0x43 ,0x2a ,0x27 ,0x2d ,0x12,
                        0x0f ,0x36 ,0x8a ,0x44 ,0x45 ,0x89 ,0x2e ,0x13 ,0x33 ,0x37 ,0x90 ,0x46 ,0x49 ,0x4c ,0x2f ,0x30,
                        0x34 ,0x38 ,0x88 ,0x47 ,0x4a ,0x4d ,0x31 ,0x32 ,0x00 ,0x87 ,0xe6 ,0x48 ,0x4b ,0x4e ,0x28 ,0x52,
                        0x50 ,0xe5 ,0xe7 ,0xd2 ,0x53 ,0x5f ,0x5c ,0x59 ,0x51 ,0x00 ,0xf1 ,0xd1 ,0x54 ,0x60 ,0x5d ,0x5a,
                        0x4f ,0x8e ,0x65 ,0xd0 ,0x55 ,0x61 ,0x5e ,0x5b ,0x62 ,0xa4 ,0xe4 ,0xfc ,0x56 ,0x57 ,0x85 ,0x58,
                        0x63 ,0x00 ,0x00 ,0xc2 ,0x24};
        internal static readonly byte[] _0x09 = new byte[] {
                        0x09 ,0x2b ,0x00 ,0x49 ,0x00 ,0x00 ,0x4a ,0x00 ,0x00 ,0x4b ,0x00 ,0x00 ,0x4c ,0x00 ,0x00 ,0x4d,
                        0x00 ,0x00 ,0x4e ,0x00 ,0x00 ,0xa4 ,0x00 ,0x00 ,0x8e ,0x00 ,0x00 ,0xd0 ,0x00 ,0x00 ,0xd1 ,0x00,
                        0x00 ,0x00 ,0x00 ,0x00 ,0x01 ,0x00 ,0x00 ,0x00 ,0x00 ,0xcd ,0x04 };
        internal static readonly byte[] _0x0d = new byte[] {
                        0x0d ,0xbb ,0x01 ,0x00 ,0x06 ,0x0b ,0x05 ,0x45 ,0x83 ,0xca ,0xca ,0xca ,0xca ,0xca ,0xca ,0xce,
                        0xce ,0xd2 ,0xce ,0xce ,0xd2 ,0x19 ,0x19 ,0x19 ,0x19 ,0x19 ,0x19 ,0x23 ,0x23 ,0x2d ,0x23 ,0x23,
                        0x2d ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe0 ,0xe3 ,0xe3 ,0xe6 ,0xe3 ,0xe3 ,0xe6 ,0xd2 ,0xd2 ,0xd5,
                        0xd2 ,0xd2 ,0xd5 ,0xd5 ,0xd5 ,0xd9 ,0xd5 ,0x00 ,0xd9 ,0x2d ,0x2d ,0x36 ,0x2d ,0x2d ,0x36 ,0x36,
                        0x36 ,0x40 ,0x36 ,0x00 ,0x40 ,0xe6 ,0xe6 ,0xe9 ,0xe6 ,0xe6 ,0xe9 ,0xe9 ,0xe9 ,0xec ,0xe9 ,0x00,
                        0xec ,0xd9 ,0xd9 ,0xdd ,0xd9 ,0xdd ,0xdd ,0xe0 ,0xe0 ,0xdd ,0xe0 ,0xe4 ,0xe4 ,0x40 ,0x40 ,0x4a,
                        0x40 ,0x4a ,0x4a ,0x53 ,0x53 ,0x4a ,0x53 ,0x5d ,0x5d ,0xec ,0xec ,0xef ,0xec ,0xef ,0xef ,0xf2,
                        0xf2 ,0xef ,0xf2 ,0xf5 ,0xf5 ,0xe4 ,0xe4 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                        0x00 ,0x5d ,0x5d ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xf5 ,0xf5 ,0x00,
                        0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xe4 ,0xe4 ,0xe8 ,0xe8 ,0xe8 ,0xe8 ,0xe8,
                        0xeb ,0xeb ,0xeb ,0x00 ,0xeb ,0x5d ,0x5d ,0x67 ,0x67 ,0x67 ,0x67 ,0x67 ,0x70 ,0x70 ,0x70 ,0x00,
                        0x70 ,0xf5 ,0xf5 ,0xf8 ,0xf8 ,0xf8 ,0xf8 ,0xf8 ,0xfb ,0xfb ,0xfb ,0x00 ,0xfb ,0xeb ,0xef ,0xef,
                        0xef ,0x00 ,0xef ,0xf0 ,0xf0 ,0xed ,0xf0 ,0xf0 ,0x00 ,0x70 ,0x7a ,0x7a ,0x7a ,0x00 ,0x7a ,0x7a,
                        0x7a ,0x6f ,0x7a ,0x7a ,0x00 ,0xfb ,0xfd ,0xfd ,0xfd ,0x00 ,0xfd ,0xf8 ,0xf8 ,0xea ,0xf8 ,0xf8,
                        0x00 ,0xed ,0xed ,0xea ,0xed ,0xed ,0x00 ,0xed ,0xea ,0xea ,0xf6 ,0xe7 ,0xea ,0x6f ,0x6f ,0x65,
                        0x6f ,0x6f ,0x00 ,0x6f ,0x65 ,0x65 ,0x66 ,0x5a ,0x65 ,0xea ,0xea ,0xdc ,0xea ,0xea ,0x00 ,0xea,
                        0xdc ,0xdc ,0x00 ,0xce ,0xdc ,0xea ,0xe7 ,0xe5 ,0xe7 ,0xe5 ,0xe5 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                        0x00 ,0x65 ,0x5a ,0x50 ,0x5a ,0x50 ,0x50 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xdc ,0xce ,0xc0,
                        0xce ,0xc0 ,0xc0 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0xe7 ,0x00 ,0x00 ,0xe2 ,0xe2 ,0xe2 ,0xe2,
                        0xdf ,0xdf ,0xdf ,0xdf ,0xdf ,0x5a ,0x00 ,0x00 ,0x45 ,0x45 ,0x45 ,0x45 ,0x3b ,0x3b ,0x3b ,0x3b,
                        0x3b ,0xce ,0x00 ,0x00 ,0xb2 ,0xb2 ,0xb2 ,0xb2 ,0xa4 ,0xa4 ,0xa4 ,0xa4 ,0xa4 ,0xdc ,0xdc ,0xdc,
                        0xdc ,0x00 ,0xda ,0xda ,0xda ,0xda ,0xda ,0x00 ,0xd7 ,0x30 ,0x30 ,0x30 ,0x30 ,0x00 ,0x26 ,0x26,
                        0x26 ,0x26 ,0x26 ,0x00 ,0x1c ,0x96 ,0x96 ,0x96 ,0x96 ,0x00 ,0x88 ,0x88 ,0x88 ,0x88 ,0x88 ,0x00,
                        0x7a ,0xd7 ,0xd7 ,0xd7 ,0x00 ,0xd4 ,0xd4 ,0xd4 ,0xd4 ,0xd4 ,0xd1 ,0xd1 ,0xd1 ,0x1c ,0x1c ,0x1c,
                        0x00 ,0x11 ,0x11 ,0x11 ,0x11 ,0x11 ,0x06 ,0x06 ,0x06 ,0x7a ,0x7a ,0x7a ,0x00 ,0x6c ,0x6c ,0x6c,
                        0x6c ,0x6c ,0x5e ,0x5e ,0x5e ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                        0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00,
                        0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x00 ,0x24 ,0xcf };
        internal static readonly byte[] _0x13 = new byte[] {
                        0x13, 0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
    }

    public enum Key
    {
        //Column 1
        ESC = 0,
        TILDE = 1,
        TAB = 2,
        CAPS_LOCK = 3,
        LEFT_SHIFT = 4,
        LEFT_CONTROL = 5,
        //Column 2
        ONE = 6,
        Q = 7,
        A = 8,
        LEFT_WINDOWS = 10,
        //Column 3
        F1 = 11,
        TWO = 12,
        W = 13,
        S = 14,
        Z = 15,
        LEFT_ALT = 16,
        //Column 4
        F2 = 17,
        THREE = 18,
        E = 19,
        D = 20,
        X = 21,
        //Column 5
        F3 = 23,
        FOUR = 24,
        R = 25,
        F = 26,
        C = 27,
        //Column 6
        F4 = 28,
        FIVE = 29,
        T = 30,
        G = 31,
        V = 32,
        //Column 7
        SIX = 33,
        Y = 34,
        H = 35,
        B = 36,
        SPACE = 37,
        //Column 8
        F5 = 48,
        SEVEN = 49,
        U = 50,
        J = 51,
        N = 52,
        //Column 9
        F6 = 53,
        EIGHT = 54,
        I = 55,
        K = 56,
        M = 57,
        //Column 10
        F7 = 59,
        NINE = 60,
        O = 61,
        L = 62,
        COMMA = 63,
        //Column 11
        F8 = 65,
        ZERO = 66,
        P = 67,
        SEMICOLON = 68,
        PERIOD = 69,
        RIGHT_ALT = 70,
        //Column 12
        MINUS = 72,
        OPEN_BRACKET = 73,
        APOSTROPHE = 74,
        FORWARD_SLASH = 75,
        FN_Key = 76,
        //Column 13
        F9 = 78,
        EQUALS = 79,
        CLOSE_BRACKET = 80,
        BACKSLASH = 81,
        RIGHT_SHIFT = 82,
        APPLICATION_SELECT = 83,
        //Column 14
        F10 = 84,
        F11 = 85,
        F12 = 86,
        BACKSPACE = 87,
        ENTER = 88,
        RIGHT_CONTROL = 89,
        //Column 15
        PRINT_SCREEN = 99,
        INSERT = 100,
        DELETE = 101,
        ARROW_LEFT = 102,
        //Column 16
        SCROLL_LOCK = 103,
        HOME = 104,
        END = 105,
        ARROW_UP = 106,
        ARROW_DOWN = 107,
        //Column 17
        PAUSE_BREAK = 108,
        PAGE_UP = 109,
        PAGE_DOWN = 110,
        ARROW_RIGHT = 111,
        //Column 18
        NUM_LOCK = 113,
        NUM_SEVEN = 114,
        NUM_FOUR = 115,
        NUM_ONE = 116,
        NUM_ZERO = 117,
        //Column 19
        NUM_SLASH = 119,
        NUM_EIGHT = 120,
        NUM_FIVE = 121,
        NUM_TWO = 122,
        //Column 20
        NUM_ASTERISK = 124,
        NUM_NINE = 125,
        NUM_SIX = 126,
        NUM_THREE = 127,
        NUM_PERIOD = 128,
        //Column 21
        NUM_MINUS = 129,
        NUM_PLUS = 130,
        NUM_ENTER = 131
    }
}
