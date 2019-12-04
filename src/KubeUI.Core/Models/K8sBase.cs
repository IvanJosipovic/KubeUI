using KubeUI.Core;
using KubeUI.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace KubeUI
{

    public partial class K8sBase
    {
        private string _apiVersion;
        private string _kind;
        private ObjectMeta _metadata;

        /// <summary>APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/api-conventions.md#resources</summary>
        [Newtonsoft.Json.JsonProperty("apiVersion", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ApiVersion
        {
            get { return _apiVersion; }
            set
            {
                if (_apiVersion != value)
                {
                    _apiVersion = value;
                }
            }
        }

        /// <summary>Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/api-conventions.md#types-kinds</summary>
        [Newtonsoft.Json.JsonProperty("kind", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Kind
        {
            get { return _kind; }
            set
            {
                if (_kind != value)
                {
                    _kind = value;
                }
            }
        }

        [Newtonsoft.Json.JsonProperty("metadata", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ObjectMeta Metadata
        {
            get { return _metadata; }
            set
            {
                if (_metadata != value)
                {
                    _metadata = value;
                }
            }
        }
    }
}
