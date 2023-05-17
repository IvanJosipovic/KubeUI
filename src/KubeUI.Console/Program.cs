namespace KubeUI.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddLogging(config => config.AddFile("Logs/{Date}.txt"));

            Core.Client.ConfigureServices.Configure(builder.Configuration, builder.Services);

            var host = builder.Build();
            host.Run();
        }
    }
}