using k8s;
using k8s.Exceptions;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace KubeUI.Services
{
    public class State : INotifyPropertyChanged, IState
    {
        public static string UILevelNotification = nameof(UILevelNotification);
        public static string ConfigNotification = nameof(ConfigNotification);
        public static string ContextNotification = nameof(ContextNotification);
        public static string NamespaceNotification = nameof(NamespaceNotification);

        private readonly ILogger<State> Logger;

        public event PropertyChangedEventHandler PropertyChanged;

        public State(ILogger<State> logger)
        {
            Logger = logger;

            try
            {
                K8SConfiguration = KubernetesClientConfiguration.LoadKubeConfig();
                Context = K8SConfiguration.CurrentContext;
            } catch(KubeConfigException)
            {
                Logger.LogInformation("No kube config found");

                K8SConfiguration = new K8SConfiguration
                {
                    Clusters = new List<Cluster>() {
                    new Cluster() { Name = "localhost:8001", ClusterEndpoint = new ClusterEndpoint() { Server = "http://localhost:8001" } }
                },
                    Contexts = new List<Context>() {
                    new Context() { Name = "localhost:8001", ContextDetails = new ContextDetails() { Cluster = "localhost:8001" } }
                }
                };

                Context = "localhost:8001";
            }
        }

        private UILevel UILevel { get; set; }

        private string _namespace;

        public string Namespace
        {
            get { return _namespace; }
            set {
                _namespace = value;
                RaisePropertyChanged(State.NamespaceNotification);
            }
        }

        private string _context;

        public string Context
        {
            get { return _context; }
            set
            {
                _context = value;
                try
                {
                    Client = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(K8SConfiguration, _context));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error creating client: {0}", ex);
                    Client = null;
                }
                RaisePropertyChanged(State.ContextNotification);
            }
        }

        public IKubernetes Client { get; set; }

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

        private K8SConfiguration K8SConfiguration;

        public void SetK8SConfiguration(string config)
        {
            K8SConfiguration = KubernetesClientConfiguration.LoadKubeConfig(config.ToStream());
            Context = K8SConfiguration.CurrentContext;

            RaisePropertyChanged(State.ConfigNotification);
        }

        public K8SConfiguration GetK8SConfiguration()
        {
            return K8SConfiguration;
        }
    }
}