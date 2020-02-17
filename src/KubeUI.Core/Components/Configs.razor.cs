using k8s;
using k8s.Models;
using KubeUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI.Core.Components
{
    public partial class Configs : IDisposable
    {
        [Inject]
        protected IState State { get; set; }

        [Inject]
        protected NavigationManager navigationManager { get; set; }

        protected override void OnInitialized()
        {
            State.PropertyChanged += (xo, e) =>
            {
                if (e.PropertyName == Services.State.ConfigNotification)
                {
                    Init();
                }
            };
            Init();
        }

        private void Init()
        {
            StateHasChanged();
        }

        private void OnChange(ChangeEventArgs args)
        {
            State.Context = args.Value.ToString();
            navigationManager.NavigateTo("/");
        }

        public void Dispose()
        {
            State.PropertyChanged -= (xo, e) =>
            {
                if (e.PropertyName == Services.State.ConfigNotification)
                {
                    Init();
                }
            };
        }
    }
}
