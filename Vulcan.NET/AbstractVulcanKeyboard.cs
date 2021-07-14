using HidSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Vulcan.NET
{
    public abstract class AbstractVulcanKeyboard : IVulcanKeyboard
    {
        private const int MaxTries = 100;
        private static readonly byte[] ColorPacketHeader = new byte[5] { 0x00, 0xa1, 0x01, 0x01, 0xb4 };

        private readonly HidDevice _ledDevice;
        private readonly HidStream _ledStream;
        private readonly HidDevice _ctrlDevice;
        private readonly HidStream _ctrlStream;
        private readonly byte[] _keyColors = new byte[444];//64 * 6 + 60

        protected abstract Dictionary<Key, int> Mapping { get; }

        public abstract KeyboardType KeyboardType { get; }

        public bool IsConnected { get; private set; }

        protected AbstractVulcanKeyboard(HidDevice ctrlDevice, HidDevice ledDevice)
        {
            _ctrlDevice = ctrlDevice;
            _ledDevice = ledDevice;

            if (!_ctrlDevice.TryOpen(out _ctrlStream) || !_ledDevice.TryOpen(out _ledStream))
            {
                throw new Exception("Failed to open devices");
            }

            if (!Initialize())
            {
                _ctrlStream?.Close();
                _ledStream?.Close();
                throw new Exception("Failed to send initialization sequence");
            }

            IsConnected = true;
        }

        public void SetColor(byte r, byte g, byte b)
        {
            foreach (Key key in Mapping.Keys)
            {
                SetKeyColor(key, r, g, b);
            }
        }

        public void SetKeyColor(Key key, byte r, byte g, byte b)
        {
            if (!Mapping.TryGetValue(key, out int keyIndex))
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

        protected abstract bool Initialize();

        protected bool GetCtrlReport(byte report_id)
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

        protected bool SetCtrlReport(byte[] reportBuffer)
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

        protected bool WaitCtrlDevice()
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
    }
}
