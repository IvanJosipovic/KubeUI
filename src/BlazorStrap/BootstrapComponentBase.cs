﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorStrap
{
    /// <summary>
    /// The base class for BlazorStrap components.
    /// </summary>
    public abstract class BootstrapComponentBase : ComponentBase
    {
        private bool _hasCalledInit;
        /// <summary>
        /// A dictionary holding any parameter name/value pairs that do not match
        /// properties declared on the component.
        /// </summary>
        protected IDictionary<string, object> UnknownParameters { get; }
            = new Dictionary<string, object>();

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterCollection parameters)
        {
            UnknownParameters.Clear();

            foreach (Parameter param in parameters)
            {
                if (TryGetPropertyInfo(param.Name, out PropertyInfo declaredPropertyInfo))
                {
                    try
                    {
                        declaredPropertyInfo.SetValue(this, param.Value);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(param.Name);
                        Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
                    }
                }
                else
                {
                    UnknownParameters[param.Name] = param.Value;
                }
            }

            if (!_hasCalledInit)
            {
                _hasCalledInit = true;
                OnInit();

                // If you override OnInitAsync and return a nonnull task, then by default
                // we automatically re-render once that task completes.
                Task initTask = OnInitAsync();
                if (initTask != null && initTask.Status != TaskStatus.RanToCompletion)
                {
                    await initTask.ContinueWith(ContinueAfterLifecycleTask);
                }
            }

            OnParametersSet();
            Task parametersTask = OnParametersSetAsync();
            if (parametersTask != null && parametersTask.Status != TaskStatus.RanToCompletion)
            {
                await parametersTask.ContinueWith(ContinueAfterLifecycleTask);
            }

            StateHasChanged();
        }

        //TODO: Only get props that are decorated by [Parameter] attribute
        private bool TryGetPropertyInfo(string propertyName, out PropertyInfo result)
        {
            result = GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return result != null;
        }

        private void ContinueAfterLifecycleTask(Task task)
        {
            if (task.Exception == null)
            {
                StateHasChanged();
            }
            else
            {
                HandleException(task.Exception);
            }
        }

        private void HandleException(Exception ex)
        {
            if (ex is AggregateException && ex.InnerException != null)
            {
                ex = ex.InnerException; // It's more useful
            }

            // TODO: Need better global exception handling
; Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
        }
    }
}
