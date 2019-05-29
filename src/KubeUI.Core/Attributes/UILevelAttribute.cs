using System;

namespace KubeUI.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class UILevelAttribute : Attribute
    {
        public UILevel UILevel { get; set; }
    }
}