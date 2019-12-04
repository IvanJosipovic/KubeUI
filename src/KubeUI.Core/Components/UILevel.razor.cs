using KubeUI.Services;
using Microsoft.AspNetCore.Components;

namespace KubeUI.Core.Components
{
    public partial class UILevel
    {
        [Inject]
        protected IState State { get; set; }
    }
}
