using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;

namespace KubeUI2.Services
{
    public interface IState
    {
        string Namespace { get; set; }

        Dictionary<Type, Collection<object>> Data { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void SetUILevel(UILevel uILevel);
        UILevel GetUILevel();
        void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null);
    }
}