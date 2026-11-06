using System.Globalization;
using Irihi.Lingua.HotReload.Minimal;

using var greeting = LanguageManager.Instance.Greeting_Message.Subscribe(new GreetingObserver());
using var culture = LanguageManager.Instance.CultureChanges.Subscribe(new CultureObserver());
var extra = LanguageManager.Instance.GetObservable("Extra_Message")?.Subscribe(new ExtraObserver());

if (extra is null)
{
    Console.WriteLine("extra:absent");
}

Console.WriteLine("ready");

using var exit = new ManualResetEventSlim();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    exit.Set();
};
exit.Wait();

extra?.Dispose();

internal sealed class GreetingObserver : IObserver<string?>
{
    public void OnNext(string? value) => Console.WriteLine($"greeting:{value ?? "<missing>"}");
    public void OnError(Exception error) => Console.WriteLine($"greeting-error:{error.Message}");
    public void OnCompleted() { }
}

internal sealed class ExtraObserver : IObserver<string?>
{
    public void OnNext(string? value) => Console.WriteLine($"extra:{value ?? "<missing>"}");
    public void OnError(Exception error) => Console.WriteLine($"extra-error:{error.Message}");
    public void OnCompleted() { }
}

internal sealed class CultureObserver : IObserver<CultureInfo>
{
    public void OnNext(CultureInfo value)
    {
        var key = LanguageManager.Instance.GetObservable("Extra_Message");
        Console.WriteLine($"extra-key:{(key is null ? "absent" : "present")}");
    }

    public void OnError(Exception error) => Console.WriteLine($"culture-error:{error.Message}");
    public void OnCompleted() { }
}
