using HidSharp;
using System.Collections.Generic;
using System.Linq;

namespace Vulcan.NET
{
    public static class VulcanFinder
    {
        private const int VendorId = 0x1E7D;

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

            foreach ((int pid, KeyboardType kbType) in ProductIds)
            {
                IEnumerable<HidDevice> devicesWithPid = devices.Where(d => d.ProductID == pid);
                if (!devicesWithPid.Any())
                {
                    continue;
                }

                switch (kbType)
                {
                    case KeyboardType.Fullsize:
                        yield return new FullsizeKeyboard(devicesWithPid);
                        break;
                    case KeyboardType.Tenkeyless:
                        yield return new TenkeylessKeyboard(devicesWithPid);
                        break;
                }
            }
        }
    }
}
