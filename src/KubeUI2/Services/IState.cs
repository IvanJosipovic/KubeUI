using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using KubeUI2;

namespace KubeUI2.Services
{
    public interface IState
    {
        Dictionary<Type, Collection<object>> Data { get; set; }

        void SetUILevel(UILevel uILevel);
        UILevel GetUILevel();

        string Namespace { get; set; }
    }
}