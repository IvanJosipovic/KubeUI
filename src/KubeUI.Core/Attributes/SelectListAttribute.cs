using System;

namespace KubeUI.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class SelectListAttribute : Attribute
    {
        public string[] Options { get; set; }
    }
}