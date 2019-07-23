using System;

namespace KubeUI.Core
{
    /// <summary>
    /// This object will required in the UI
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredAttribute : Attribute
    {
    }
}