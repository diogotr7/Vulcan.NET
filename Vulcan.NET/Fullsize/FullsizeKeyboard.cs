using HidSharp;
using System.Collections.Generic;

namespace Vulcan.NET
{
    public sealed class FullsizeKeyboard : AbstractVulcanKeyboard
    {
        public override KeyboardType KeyboardType => KeyboardType.Fullsize;

        protected override Dictionary<Key, int> Mapping => FullSizeKeyMapping.Mapping;

        internal FullsizeKeyboard(HidDevice ctrlDevice, HidDevice ledDevice)
            : base(ctrlDevice, ledDevice)
        { }

        protected override bool Initialize()
        {
            return
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
        }
    }
}