using KubeUI.Console;
using KubeUI.Core.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((host, services) =>
    {
        services.AddLogging(config => config.AddFile("Logs/{Date}.txt"));

        services.AddHostedService<Worker>();

        ConfigureServices.Configure(host.Configuration, services);
    })
    .Build();

await host.RunAsync();
