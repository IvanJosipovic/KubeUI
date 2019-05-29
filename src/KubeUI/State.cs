using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using FluentValidation;
using KubeUI.Core;
using KubeUI.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KubeUI
{
    public class State : INotifyPropertyChanged, IState
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            Error = (object _, Newtonsoft.Json.Serialization.ErrorEventArgs args) => args.ErrorContext.Handled = true
        };

        private readonly ILogger<State> Logger;

        private readonly LocalStorage Storage;

        private readonly IValidatorFactory ValidatorFactory;

        public State(ILogger<State> logger)
        {
            Logger = logger;
        }

        public State(ILogger<State> logger, IValidatorFactory validatorFactory, LocalStorage localStorage)
        {
            this.Logger = logger;
            this.ValidatorFactory = validatorFactory;
            this.Storage = localStorage;

            LoadState();

            // Preload default objects
            GetCollection(typeof(ConfigMap));
            GetCollection(typeof(CronJob));
            GetCollection(typeof(DaemonSet));
            GetCollection(typeof(Deployment));
            GetCollection(typeof(Ingress2));
            GetCollection(typeof(PersistentVolumeClaim));
            GetCollection(typeof(Secret));
            GetCollection(typeof(Service));
            GetCollection(typeof(StatefulSet));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<Type, Collection<object>> Data { get; set; } = new Dictionary<Type, Collection<object>>();

        private UILevel UILevel { get; set; }

        public void AddExistingItem(Type type, object obj)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();

            object[] data = { obj };

            collType.GetMethod("Add").Invoke(collection, data);
        }

        public int AddItem(Type type)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();
            var count = (int)collType.GetProperty("Count").GetValue(collection);

            var item = CreateObject(type);
            object[] data = { item };

            collType.GetMethod("Add").Invoke(collection, data);

            return count;
        }

        public object CreateObject(Type type)
        {
            var count = GetCount(type);

            if (type == typeof(ConfigMap))
            {
                var item = new Schema.ConfigMap()
                {
                    ApiVersion = "v1",
                    Kind = "ConfigMap",
                    Metadata = new ObjectMeta()
                    {
                        Name = "ConfigMap " + count
                    },
                };

                return item;
            }
            else if (type == typeof(CronJob))
            {
                var item = new Schema.CronJob()
                {
                    ApiVersion = "v1beta1",
                    Kind = "CronJob",
                    Metadata = new ObjectMeta()
                    {
                        Name = "CronJob " + count
                    },
                };

                return item;
            }
            else if (type == typeof(DaemonSet))
            {
                var item = new Schema.DaemonSet()
                {
                    ApiVersion = "apps/v1",
                    Kind = "DaemonSet",
                    Metadata = new ObjectMeta()
                    {
                        Name = "DaemonSet " + count
                    }
                };
                return item;
            }
            else if (type == typeof(Deployment))
            {
                var item = new Schema.Deployment()
                {
                    ApiVersion = "apps/v1",
                    Kind = "Deployment",
                    Metadata = new ObjectMeta()
                    {
                        Name = "Deployment " + count
                    }
                };
                return item;
            }
            else if (type == typeof(Ingress2))
            {
                var item = new Schema.Ingress2()
                {
                    ApiVersion = "extensions/v1beta1",
                    Kind = "Ingress",
                    Metadata = new ObjectMeta()
                    {
                        Name = "Ingress " + count
                    }
                };
                return item;
            }
            else if (type == typeof(PersistentVolumeClaim))
            {
                var item = new Schema.PersistentVolumeClaim()
                {
                    ApiVersion = "v1",
                    Kind = "PersistentVolumeClaim",
                    Metadata = new ObjectMeta()
                    {
                        Name = "PersistentVolumeClaim " + count
                    }
                };
                return item;
            }
            else if (type == typeof(Secret))
            {
                var item = new Schema.Secret()
                {
                    ApiVersion = "v1",
                    Kind = "Secret",
                    Metadata = new ObjectMeta()
                    {
                        Name = "Secret " + count
                    },
                };
                return item;
            }
            else if (type == typeof(Service))
            {
                var item = new Schema.Service()
                {
                    ApiVersion = "v1",
                    Kind = "Service",
                    Metadata = new ObjectMeta()
                    {
                        Name = "Service " + count
                    },
                };
                return item;
            }
            else if (type == typeof(StatefulSet))
            {
                var item = new Schema.StatefulSet()
                {
                    ApiVersion = "apps/v1",
                    Kind = "StatefulSet",
                    Metadata = new ObjectMeta()
                    {
                        Name = "StatefulSet " + count
                    }
                };
                return item;
            }

            throw new Exception("Type not supported: " + type);
        }

        public void DeleteItem(Type type, int Id)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();
            object[] data = { Id };

            collType.GetMethod("RemoveAt").Invoke(collection, data);
            RaisePropertyChanged();
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
            var count = (int)collType.GetProperty("Count").GetValue(collection);

            return count;
        }

        public object GetItem(Type type, int Id)
        {
            var collection = GetCollection(type);
            var collType = collection.GetType();

            object[] index = { Id };
            var item = collType.GetProperty("Item").GetValue(collection, index);

            return item;
        }

        public UILevel GetUILevel()
        {
            return UILevel;
        }

        public void ImportObject(string data)
        {
            var serializer = new SerializerBuilder()
                .WithTypeInspector(x => new JsonPropertyTypeInspector(x))
                .JsonCompatible()
                .Build();

            var deserializer = new DeserializerBuilder()
                .WithTypeInspector(x => new JsonPropertyTypeInspector(x))
                .Build();

            if ((data.StartsWith("{") && data.EndsWith("}")) || (data.StartsWith("[") && data.EndsWith("]")))
            {
                // data is Json
                Add(data);
            }
            else
            {
                // data is  Yaml, need to convert to Json
                using (TextReader textReader = new StringReader(data))
                {
                    IParser parser = new Parser(textReader);
                    parser.Expect<StreamStart>();

                    while (parser.Accept<DocumentStart>())
                    {
                        try
                        {
                            var yamlObject = deserializer.Deserialize(parser);
                            var json = serializer.Serialize(yamlObject);
                            Add(json);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error Loading: {0} \n {1}", ex, data);
                        }
                    }
                }
            }

            void Add(string json)
            {
                // Deserialize the document into the base type so we can read ApiVersion and Kind
                var k8sbase = JsonConvert.DeserializeObject<K8sBase>(json, JsonSettings);

                string key = k8sbase.ApiVersion + "-" + k8sbase.Kind;

                switch (key)
                {
                    case "v1-ConfigMap":
                        AddExistingItem(typeof(ConfigMap), JsonConvert.DeserializeObject<ConfigMap>(json, JsonSettings));
                        break;

                    case "batch/v1beta1-CronJob":
                        AddExistingItem(typeof(CronJob), JsonConvert.DeserializeObject<CronJob>(json, JsonSettings));
                        break;

                    case "apps/v1-DaemonSet":
                        AddExistingItem(typeof(DaemonSet), JsonConvert.DeserializeObject<DaemonSet>(json, JsonSettings));
                        break;

                    case "apps/v1-Deployment":
                        AddExistingItem(typeof(Deployment), JsonConvert.DeserializeObject<Deployment>(json, JsonSettings));
                        break;

                    case "extensions/v1beta1-Ingress":
                        AddExistingItem(typeof(Ingress2), JsonConvert.DeserializeObject<Ingress2>(json, JsonSettings));
                        break;

                    case "v1-PersistentVolumeClaim":
                        AddExistingItem(typeof(PersistentVolumeClaim), JsonConvert.DeserializeObject<PersistentVolumeClaim>(json, JsonSettings));
                        break;

                    case "v1-Secret":
                        AddExistingItem(typeof(Secret), JsonConvert.DeserializeObject<Secret>(json, JsonSettings));
                        break;

                    case "v1-Service":
                        AddExistingItem(typeof(Service), JsonConvert.DeserializeObject<Service>(json, JsonSettings));
                        break;

                    case "apps/v1-StatefulSet":
                        AddExistingItem(typeof(StatefulSet), JsonConvert.DeserializeObject<StatefulSet>(json, JsonSettings));
                        break;

                    default:
                        Logger.LogError("{0} - Object Type not supported", key);
                        break;
                }
            }
        }

        public bool IsValid<T>(T item)
        {
            var validator = ValidatorFactory.GetValidator(typeof(T));
            //Console.WriteLine($"IsValid {typeof(T)}");
            if (validator != null)
            {
                return validator.Validate(item).IsValid;
            }
            //Console.WriteLine($"IsValid validator not found {typeof(T)}");

            return true;
        }

        public bool IsValid(object item, Type type)
        {
            var validator = ValidatorFactory.GetValidator(type);
            //Console.WriteLine($"IsValid {type}");
            if (validator != null)
            {
                return validator.Validate(item).IsValid;
            }
            //Console.WriteLine($"IsValid validator not found {type}");

            return true;
        }

        public void LoadState()
        {
            var json = Storage["Data"];
            if (!String.IsNullOrEmpty(json))
            {
                Logger.LogInformation("LogState - Found existing state.");
                try
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<Type, Collection<object>>>(json, JsonSettings);
                }
                catch (Exception ex)
                {
                    Logger.LogError("LoadState Error {0}", ex);
                }
            }
        }

        public virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            SaveState();
        }

        public void SaveState()
        {
            Storage["Data"] = JsonConvert.SerializeObject(Data, JsonSettings);
        }

        public void SetUILevel(UILevel uILevel)
        {
            UILevel = uILevel;
            RaisePropertyChanged(nameof(UILevel));
        }
    }
}