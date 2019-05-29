namespace KubeUI
{
    public interface IAppInsights
    {
        void TrackEvent(string name);
    }
}