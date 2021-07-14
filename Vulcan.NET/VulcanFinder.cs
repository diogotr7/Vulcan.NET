using HidSharp;
using HidSharp.Reports.Encodings;
using System.Collections.Generic;
using System.Linq;

namespace Vulcan.NET
{
    public static class VulcanFinder
    {
        private const int VendorId = 0x1E7D;
        private const uint LedUsagePage = 0x0001;
        private const uint LedUsage = 0x0000;

        private static readonly List<(int, KeyboardType)> ProductIds = new List<(int, KeyboardType)>
        {
            (0x307A, KeyboardType.Fullsize),
            (0x3098, KeyboardType.Fullsize),
            (0x2fee, KeyboardType.Tenkeyless)
        };

        public static IEnumerable<IVulcanKeyboard> FindKeyboards()
        {
            IEnumerable<HidDevice> devices = DeviceList.Local.GetHidDevices(vendorID: VendorId);
            if (!devices.Any())
            {
                yield break;
            }

            foreach (var distinctDeviceEntries in devices.GroupBy(d => d.DevicePath))
            {
                foreach ((int pid, KeyboardType kbType) in ProductIds)
                {
                    //these have the same pid
                    if (!distinctDeviceEntries.Any(dde => dde.ProductID == pid))
                    {
                        continue;
                    }

                    var ctrlDevice = distinctDeviceEntries.FirstOrDefault(d => d.GetMaxFeatureReportLength() > 50);
                    var ledDevice = GetFromUsages(devices, LedUsagePage, LedUsage);

                    if (ctrlDevice == null || ledDevice == null)
                        continue;

                    switch (kbType)
                    {
                        case KeyboardType.Fullsize:
                            yield return new FullsizeKeyboard(ctrlDevice, ledDevice);
                            break;
                        case KeyboardType.Tenkeyless:
                            yield return new TenkeylessKeyboard(ctrlDevice, ledDevice);
                            break;
                    }
                }
            }
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
