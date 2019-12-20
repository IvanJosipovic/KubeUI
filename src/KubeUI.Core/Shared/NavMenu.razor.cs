﻿using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;

namespace KubeUI.Core.Shared
{
    public partial class NavMenu : IDisposable
    {
        [Inject]
        private IState State { get; set; }

        private bool collapseNavMenu = true;

        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        private PropertyChangedEventHandler handler;

        protected override void OnInitialized()
        {
            handler = (xo, e) =>
            {
                if (e.PropertyName == Services.State.UILevelNotification || e.PropertyName == Services.State.NamespaceNotification)
                {
                    StateHasChanged();
                }
            };

            State.PropertyChanged += handler;
        }

        public void Dispose()
        {
            State.PropertyChanged -= handler;
        }
    }
}