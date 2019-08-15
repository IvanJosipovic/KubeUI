namespace KubeUI.Services
{
    public interface IAppInsights
    {
        void TrackEvent(string name);
    }
}