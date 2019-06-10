using System;

namespace KubeUI.Core
{
    /// <summary>
    /// This object will not be visible in the UI
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}