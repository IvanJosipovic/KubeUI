using KubeUI.Core.Components.Dynamic;
using MudBlazor;
using MudBlazor.Services;
using System.Linq;
using System.Threading.Tasks;

namespace KubeUI.Core.Tests;

public class DynamicControlTests : TestContext
{
    public DynamicControlTests()
    {
        Services.AddSingleton<IKeyInterceptorFactory, MockKeyInterceptorServiceFactory>();
    }

    private class UI1
    {
        public string StringControl { get; set; }

        public bool BoolControl { get; set; }

        public double DoubleControl { get; set; }

        public int IntControl { get; set; }

        public long LongControl { get; set; }
    }

    [Fact]
    public void SetStringValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudTextField<string>>();

        var input = textField.Find("input");

        input.Change("test");

        model.StringControl.Should().Be("test");
    }

    [Fact]
    public void SetBoolValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudSwitch<bool?>>();

        var input = textField.Find("input");

        input.Change(true);

        model.BoolControl.Should().Be(true);
    }

    [Fact]
    public void SetDoubleValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudNumericField<double?>>();

        var input = textField.Find("input");

        input.Change(1.23);

        model.DoubleControl.Should().Be(1.23);
    }

    [Fact]
    public void SetIntValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudNumericField<int?>>();

        var input = textField.Find("input");

        input.Change(int.MaxValue);

        model.IntControl.Should().Be(int.MaxValue);
    }

    [Fact]
    public void SetLongValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudNumericField<long?>>();

        var input = textField.Find("input");

        input.Change(long.MaxValue);

        model.LongControl.Should().Be(long.MaxValue);
    }
}

#pragma warning disable CS1998
#pragma warning disable CS0067
public class MockKeyInterceptorServiceFactory : IKeyInterceptorFactory
{
    private readonly MockKeyInterceptorService _interceptorService;

    public MockKeyInterceptorServiceFactory(MockKeyInterceptorService interceptorService)
    {
        _interceptorService = interceptorService;
    }

    public MockKeyInterceptorServiceFactory()
    {

    }

    public IKeyInterceptor Create() => _interceptorService ?? new MockKeyInterceptorService();
}

public class MockKeyInterceptorService : IKeyInterceptor
{
    public void Dispose()
    {

    }

    public Task Connect(string element, KeyInterceptorOptions options)
    {
        return Task.CompletedTask;
    }

    public Task Disconnect()
    {
        return Task.CompletedTask;
    }

    public Task UpdateKey(KeyOptions option)
    {
        return Task.CompletedTask;
    }

    public event KeyboardEvent KeyDown;
    public event KeyboardEvent KeyUp;
}
