using KubeUI2;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace KubeUI2.Services
{
    public class State : INotifyPropertyChanged, IState
    {
        public static string UILevelNotification = nameof(UILevelNotification);
        public static string NamespaceNotification = nameof(NamespaceNotification);

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

            // Preload default objects
            //GetCollection(typeof(ConfigMap));
            //GetCollection(typeof(CronJob));
            //GetCollection(typeof(DaemonSet));
            //GetCollection(typeof(Deployment));
            //GetCollection(typeof(Ingress2));
            //GetCollection(typeof(PersistentVolumeClaim));
            //GetCollection(typeof(Secret));
            //GetCollection(typeof(Service));
            //GetCollection(typeof(StatefulSet));
        }

        public Dictionary<Type, Collection<object>> Data { get; set; } = new Dictionary<Type, Collection<object>>();

        private UILevel UILevel { get; set; }

        private string _namespace = "default";

        public string Namespace
        {
            get { return _namespace; }
            set { 
                _namespace = value;
                RaisePropertyChanged(State.NamespaceNotification);
            }
        }

        public void DeleteItem(Type type, int Id)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();
            object[] data = { Id };

            collType.GetMethod("RemoveAt").Invoke(collection, data);
        }

        public object GetCollection(Type type)
        {
            if (Data.TryGetValue(type, out Collection<object> items))
            {
                return items;
            }

            var coll = new Collection<object>();

            Data.Add(type, coll);
            return coll;
        }

        public int GetCount(Type type)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();
            return (int)collType.GetProperty("Count").GetValue(collection);
        }

        public object GetItem(Type type, int Id)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();

            var count = (int)collType.GetProperty("Count").GetValue(collection);

            if (Id < count)
            {
                return collType.GetProperty("Item").GetValue(collection, new object[] { Id });
            }

            return null;
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