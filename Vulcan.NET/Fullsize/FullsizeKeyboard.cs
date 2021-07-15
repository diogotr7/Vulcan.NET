using HidSharp;
using System.Collections.Generic;

namespace Vulcan.NET
{
    /// <summary>
    /// Represents a full size vulcan keyboard. Vulcan 100/120
    /// </summary>
    public sealed class FullsizeKeyboard : AbstractVulcanKeyboard
    {
        /// <inheritdoc/>
        public override KeyboardType KeyboardType => KeyboardType.Fullsize;

        /// <inheritdoc/>
        protected override Dictionary<Key, int> Mapping => FullSizeKeyMapping.Mapping;

        internal FullsizeKeyboard(HidDevice ctrlDevice, HidDevice ledDevice)
            : base(ctrlDevice, ledDevice)
        { }

        /// <inheritdoc/>
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