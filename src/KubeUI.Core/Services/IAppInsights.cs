using System.Threading.Tasks;

namespace KubeUI.Services
{
    public interface IAppInsights
    {
        Task TrackEvent(string name);
    }
}