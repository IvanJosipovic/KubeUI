using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KubeUI.Services
{
    public class State : INotifyPropertyChanged, IState
    {
        public static string UILevelNotification = nameof(UILevelNotification);
        public static string NamespaceNotification = nameof(NamespaceNotification);
        public static string AllNameSpace = "ALL";

        private readonly ILogger<State> Logger;

        private readonly IJSRuntime JSRuntime;

        private readonly IAppInsights appInsights;

        public event PropertyChangedEventHandler PropertyChanged;

        public State(ILogger<State> logger)
        {
            Logger = logger;
        }

        public State(ILogger<State> logger, IJSRuntime JSRuntime, IAppInsights appInsights)
        {
            this.Logger = logger;
            this.JSRuntime = JSRuntime;
            this.appInsights = appInsights;

        }

        public Dictionary<Type, Collection<object>> Data { get; set; } = new Dictionary<Type, Collection<object>>();

        private UILevel UILevel { get; set; }

        private string _namespace = AllNameSpace;

        public string Namespace
        {
            get { return _namespace; }
            set { 
                _namespace = value;
                RaisePropertyChanged(State.NamespaceNotification);
            }
        }

        public UILevel GetUILevel()
        {
            return UILevel;
        }

        public void SetUILevel(UILevel uILevel)
        {
            UILevel = uILevel;
            RaisePropertyChanged(State.UILevelNotification);
        }

        public virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}