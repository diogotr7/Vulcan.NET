﻿using HidSharp;
using System.Collections.Generic;

namespace Vulcan.NET
{
    /// <summary>
    /// Represents a tenkeyless vulcan keyboard. Vulcan TKL
    /// </summary>
    public sealed class TenkeylessKeyboard : AbstractVulcanKeyboard
    {
        /// <inheritdoc/>
        public override KeyboardType KeyboardType => KeyboardType.Tenkeyless;

        /// <inheritdoc/>
        protected override Dictionary<Key, int> Mapping => TenkeylessKeyMapping.Mapping;

        internal TenkeylessKeyboard(HidDevice ctrlDevice, HidDevice ledDevice)
            : base(ctrlDevice, ledDevice)
        { }

        /// <inheritdoc/>
        protected override bool Initialize()
        {
            return
                GetCtrlReport(0x0f) &&
                SetCtrlReport(TenkeylessCtrlReports._0x04) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x15) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x05) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x0a) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x0b) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x06) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x09) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x0d) &&
                WaitCtrlDevice() &&
                SetCtrlReport(TenkeylessCtrlReports._0x13) &&
                WaitCtrlDevice();
        }
    }
}