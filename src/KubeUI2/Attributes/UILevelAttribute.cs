using System;

namespace KubeUI.Core
{
    /// <summary>
    /// The control will only be visible on this UILevel or above
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class UILevelAttribute : Attribute
    {
        /// <summary>
        /// Which UILevel this control should be displayed in
        /// </summary>
        public UILevel UILevel { get; set; }
    }
}