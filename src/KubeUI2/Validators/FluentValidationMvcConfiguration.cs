#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at https://raw.githubusercontent.com/JeremySkinner/FluentValidation/master/src/FluentValidation.AspNetCore/FluentValidationMvcConfiguration.cs
#endregion

namespace KubeUI.Validators
{
    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// FluentValidation asp.net core configuration
    /// </summary>
    public class FluentValidationMvcConfiguration
    {
        /// <summary>
        /// The type of validator factory to use. Uses the ServiceProviderValidatorFactory by default.
        /// </summary>
        public Type ValidatorFactoryType { get; set; }

        /// <summary>
        /// The validator factory to use. Uses the ServiceProviderValidatorFactory by default. 
        /// </summary>
        public IValidatorFactory ValidatorFactory { get; set; }

        /// <summary>
        /// Enables or disables localization support within FluentValidation
        /// </summary>
        public bool LocalizationEnabled
        {
            get => ValidatorOptions.LanguageManager.Enabled;
            set => ValidatorOptions.LanguageManager.Enabled = value;
        }

        internal List<Assembly> AssembliesToRegister { get; } = new List<Assembly>();

        /// <summary>
        /// Registers all validators derived from AbstractValidator within the assembly containing the specified type
        /// </summary>
        public FluentValidationMvcConfiguration RegisterValidatorsFromAssemblyContaining<T>()
        {
            return RegisterValidatorsFromAssemblyContaining(typeof(T));
        }

        /// <summary>
        /// Registers all validators derived from AbstractValidator within the assembly containing the specified type
        /// </summary>
        public FluentValidationMvcConfiguration RegisterValidatorsFromAssemblyContaining(Type type)
        {
            return RegisterValidatorsFromAssembly(type.GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Registers all validators derived from AbstractValidator within the specified assembly
        /// </summary>
        public FluentValidationMvcConfiguration RegisterValidatorsFromAssembly(Assembly assembly)
        {
            ValidatorFactoryType = typeof(ServiceProviderValidatorFactory);
            AssembliesToRegister.Add(assembly);
            return this;
        }
    }
}