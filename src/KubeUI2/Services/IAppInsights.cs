namespace KubeUI2.Services
{
    public interface IAppInsights
    {
        void TrackEvent(string name);
    }
}