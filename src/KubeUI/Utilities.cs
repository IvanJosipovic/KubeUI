using Microsoft.Extensions.DependencyInjection;

namespace KubeUI;

public static class Utilities
{
    public static T GetRequiredService<T>(this Application? app)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service).GetRequiredService<T>();
        }
        else
        {
            throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
        }
    }

    public static object GetRequiredService(this Application? app, Type type)
    {
        if (app.TryFindResource(typeof(IServiceProvider), out var service))
        {
            return ((IServiceProvider)service!).GetRequiredService(type);
        }
        else
        {
            throw new Exception($"Cant find {typeof(IServiceProvider).Name}");
        }
    }
}
