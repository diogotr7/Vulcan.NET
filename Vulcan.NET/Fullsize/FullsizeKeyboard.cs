using HidSharp;
using HidSharp.Reports.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Vulcan.NET
{
    public class FullsizeKeyboard : IVulcanKeyboard
    {
        private const uint LedUsagePage = 0x0001;
        private const uint LedUsage = 0x0000;
        private const int MaxTries = 100;
        private static readonly byte[] ColorPacketHeader = new byte[5] { 0x00, 0xa1, 0x01, 0x01, 0xb4 };

        private readonly HidDevice _ledDevice;
        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;
        private readonly byte[] _keyColors = new byte[444];//64 * 6 + 60

        public KeyboardType KeyboardType => KeyboardType.Fullsize;

        public bool IsConnected { get; private set; }

        internal FullsizeKeyboard(IEnumerable<HidDevice> devices)
        {
            _ctrlDevice = devices.FirstOrDefault(d => d.GetMaxFeatureReportLength() > 50);
            _ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);

            if (_ctrlDevice == null || _ledDevice == null)
            {
                throw new Exception("Failed to find expected devices from report length and usages");
            }

            if (!_ctrlDevice.TryOpen(out _ctrlStream) || !_ledDevice.TryOpen(out _ledStream))
            {
                throw new Exception("Failed to open devices");
            }

            if (!SendCtrlInitSequence())
            {
                throw new Exception("Failed to send initialization sequence");
            }

            IsConnected = true;
        }

        public void SetColor(byte r, byte g, byte b)
        {
            foreach (Key key in FullSizeKeyMapping.Mapping.Keys)
            {
                SetKeyColor(key, r, g, b);
            }
        }

        public void SetKeyColor(Key key, byte r, byte g, byte b)
        {
            if (!FullSizeKeyMapping.Mapping.TryGetValue(key, out int keyIndex))
            {
                return;
            }

            int offset = (keyIndex / 12 * 36) + (keyIndex % 12);
            _keyColors[offset + 0] = r;
            _keyColors[offset + 12] = g;
            _keyColors[offset + 24] = b;
        }

        public bool Update()
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
                return false;
            }
        }

        public void Dispose()
        {
            _ctrlStream?.Close();
            _ledStream?.Close();
            IsConnected = false;
        }

        private bool SendCtrlInitSequence()
        {
            bool result =
                GetCtrlReport(0x0f) &&
                SetCtrlReport(FullsizeCtrlReports._0x15) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x05) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x07) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x0a) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x0b) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x06) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x09) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x0d) &&
                WaitCtrlDevice() &&
                SetCtrlReport(FullsizeCtrlReports._0x13) &&
                WaitCtrlDevice();

            _ctrlStream?.Close();

            return result;
        }

        private bool GetCtrlReport(byte report_id)
        {
            int size = _ctrlDevice.GetMaxFeatureReportLength();
            byte[] buf = new byte[size];
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
                    {
                        return true;
                    }
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
            foreach (HidDevice dev in devices)
            {
                try
                {
                    byte[] raw = dev.GetRawReportDescriptor();
                    IEnumerable<EncodedItem> usages = EncodedItem.DecodeItems(raw, 0, raw.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);

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
}