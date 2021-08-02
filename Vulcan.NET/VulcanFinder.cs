using HidSharp;
using HidSharp.Reports.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vulcan.NET
{
    /// <summary>
    /// Static class to find all connected keyboards.
    /// </summary>
    public static class VulcanFinder
    {
        private const int VendorId = 0x1E7D;
        private const uint LedUsagePage = 0x0001;
        private const uint LedUsage = 0x0000;

        private static readonly List<(int, KeyboardType)> KeyboardDefinitions = new List<(int, KeyboardType)>
        {
            (0x307A, KeyboardType.Fullsize),//100/120
            (0x3098, KeyboardType.Fullsize),//100/120
            (0x2fee, KeyboardType.Tenkeyless)//tkl
            //tkl pro 0x311a
            //pro     0x30f7
        };

        /// <summary>
        /// Returns an enumeration of all keyboards found.
        /// </summary>
        public static IEnumerable<IVulcanKeyboard> FindKeyboards()
        {
            IEnumerable<HidDevice> devices = DeviceList.Local.GetHidDevices(vendorID: VendorId);
            if (!devices.Any())
            {
                yield break;
            }

            foreach ((int pid, KeyboardType kbType) in KeyboardDefinitions)
            {
                var devicesWithPid = devices.Where(d => d.ProductID == pid);

                var ctrlDevice = devicesWithPid.FirstOrDefault(d => d.GetMaxFeatureReportLength() > 50);
                var ledDevice = devicesWithPid.FirstOrDefault(d => d.VerifyUsageAndUsagePage(LedUsagePage, LedUsage));

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

        private static bool VerifyUsageAndUsagePage(this HidDevice device, uint usagePage, uint usage)
        {
            try
            {
                var rawReportDescriptor = device.GetRawReportDescriptor();

                var items = EncodedItem.DecodeItems(rawReportDescriptor, 0, rawReportDescriptor.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);

                return items.Any(item => item.ItemType == ItemType.Global && item.DataValue == usagePage)
                    && items.Any(item => item.ItemType == ItemType.Local && item.DataValue == usage);
            }
            catch
            {
                return false;
            }
        }
    }
}
