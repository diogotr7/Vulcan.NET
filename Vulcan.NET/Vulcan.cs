using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports.Encodings;

namespace Vulcan.NET
{
    /// <summary>
    /// Class representing a vulcan Keyboard. Can only interface with one at a time
    /// </summary>
    public class VulcanKeyboard : IDisposable
    {
        private const int MaxTries = 100;
        private const int VendorId = 0x1E7D;
        private const uint LedUsagePage = 0x0001;
        private const uint LedUsage = 0x0000;
        private static readonly int[] ProductIds = new int[] { 0x307A, 0x3098 };
        private static readonly byte[] ColorPacketHeader = new byte[5] { 0x00, 0xa1, 0x01, 0x01, 0xb4 };

        private readonly HidDevice _ledDevice;
        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;
        private readonly byte[] _keyColors = new byte[444];//64 * 6 + 60

        public VulcanKeyboard(HidDevice ledDevice, HidStream ledStream, HidDevice ctrlDevice, HidStream ctrlStream)
        {
            _ledDevice = ledDevice;
            _ledStream = ledStream;
            _ctrlDevice = ctrlDevice;
            _ctrlStream = ctrlStream;
        }

        /// <summary>
        /// Initializes the keyboard. Returns a keyboard object if initialized successfully or null otherwise
        /// </summary>
        public static VulcanKeyboard Initialize()
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId)
                        .Where(d => ProductIds.Any(id => id == d.ProductID));

            if (!devices.Any())
                return null;

            try
            {
                HidDevice ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);
                HidDevice ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);
                HidStream ledStream = null;
                HidStream ctrlStream = null;

                if ((ctrlDevice?.TryOpen(out ctrlStream) ?? false) && (ledDevice?.TryOpen(out ledStream) ?? false))
                {
                    VulcanKeyboard kb = new VulcanKeyboard(ledDevice,ledStream,ctrlDevice,ctrlStream);
                    if (kb.SendCtrlInitSequence())
                        return kb;
                }
                else
                {
                    ctrlStream?.Close();
                    ledStream?.Close();
                }
            }
            catch
            { }

            return null;
        }

        /// <summary>
        /// A Proxy for the <see cref="Disconnect"/>
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        #region Public Methods
        /// <summary>
        /// Sets the whole keyboard to a color
        /// </summary>
        public void SetColor(Color clr)
        {
            foreach (Key key in (Key[])Enum.GetValues(typeof(Key)))
                SetKeyColor(key, clr);
        }

        /// <summary>
        /// Set the colors of all the keys in the dictionary
        /// </summary>
        public void SetColors(Dictionary<Key, Color> keyColors)
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        /// <summary>
        /// Sets a given key to a given color
        /// </summary>
        public void SetKeyColor(Key key, Color clr)
        {
            int offset = ((int)key / 12 * 36) + ((int)key % 12);
            _keyColors[offset + 0] = clr.R;
            _keyColors[offset + 12] = clr.G;
            _keyColors[offset + 24] = clr.B;
        }

        /// <summary>
        /// Writes data to the keyboard
        /// </summary>
        public bool Update() => WriteColorBuffer();

        /// <summary>
        /// Disconnects from the keyboard. Call this last
        /// </summary>
        public void Disconnect()
        {
            _ctrlStream?.Close();
            _ledStream?.Close();
        }

        #endregion

        #region Private Hid Methods
        private bool WriteColorBuffer()
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

        private bool SendCtrlInitSequence()
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

        private bool GetCtrlReport(byte report_id)
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            var buf = new byte[size];
            buf[0] = report_id;
            try
            {
                _ctrlStream.GetFeature(buf);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SetCtrlReport(byte[] reportBuffer)
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

        private bool WaitCtrlDevice()
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
        #endregion

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
                            return dev;
                        }
                    }
                }
                catch
                {
                    //failed to get the report descriptor, skip
                }
            }
            return null;
        }
    }
}
