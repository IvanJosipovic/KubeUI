using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.IO;
using k8s.KubeConfigModels;
using System.Runtime.CompilerServices;
using k8s;

namespace KubeUI.Services
{
    public interface IState
    {
        IKubernetes Client { get; set; }

        string Context { get; set; }

        string Namespace { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void SetUILevel(UILevel uILevel);

        UILevel GetUILevel();

        void SetK8SConfiguration(string config);

        K8SConfiguration GetK8SConfiguration();

        void RaisePropertyChanged([CallerMemberName] string propertyName = null);
    }
}