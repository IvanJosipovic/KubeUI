using KubeUI.UI.Components;
using KubeUI.UI.Components.Dynamic;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KubeUI.Core.Client.HelmRelease;

namespace KubeUI.UI.Tests;

public class DynamicControlTests : TestContext
{
    public DynamicControlTests()
    {
        Services.AddSingleton<IKeyInterceptorFactory, MockKeyInterceptorServiceFactory>();
        Services.AddSingleton<IScrollListener, MockScrollListener>();
        Services.AddSingleton<IScrollManager, MockScrollManager>();
        Services.AddSingleton<IBrowserWindowSizeProvider, MockBrowserWindowSizeProvider>();
        Services.AddSingleton<IMudPopoverService, MockPopoverService>();
        Services.AddSingleton<IPopoverService, MockPopoverServiceV2>();
        Services.AddSingleton(x => Mock.Of<IJsApiService>());
    }

    private class UI1
    {
        public string StringControl { get; set; }

        public bool BoolControl { get; set; }

        public double DoubleControl { get; set; }

        public int IntControl { get; set; }

        public long LongControl { get; set; }

        public DateTime DateTimeControl { get; set; }

        //public byte[] ByteArrayControl { get; set; }
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

    //[Fact]
    // todo fix test
    public void SetDateTimeValue()
    {
        var model = new UI1();

        var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

        var textField = cmpt.FindComponent<MudDatePicker>();

        var input = textField.Find("input");

        var date = DateTime.UtcNow;

        input.Change(date.ToString());

        model.DateTimeControl.Should().Be(date);
    }

    //[Fact]
    // todo fix test. This test requires JavaScript due to BlazorMonaco
    //public void SetByteArrayValue()
    //{
    //    var model = new UI1();

    //    var cmpt = RenderComponent<Controls<UI1>>(param => param.Add(p => p.Item, model));

    //    var textField = cmpt.FindComponent<KubeMonacoEditor>();

    //    var input = textField.Find("input");

    //    input.Change("test");

    //    model.ByteArrayControl.Should().BeEquivalentTo(Encoding.UTF8.GetBytes("test"));
    //}
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

public class MockScrollListenerFactory : IScrollListenerFactory
{
    public IScrollListener Create(string selector) =>
        new MockScrollListener()
        {
            Selector = selector,
        };
}

public class MockScrollListener : IScrollListener
{
    public string Selector { get; set; }

    public event EventHandler<ScrollEventArgs> OnScroll;

    public MockScrollListener()
    {
        OnScroll?.Invoke(this, new ScrollEventArgs());
    }

    public void Dispose()
    {
    }
}

public class MockScrollManager : IScrollManager
{
    public string Selector { get; set; }

    public ValueTask LockScrollAsync(string elementId, string cssClass) => ValueTask.CompletedTask;

    public Task ScrollTo(int left, int top, ScrollBehavior scrollBehavior) => Task.CompletedTask;

    public ValueTask ScrollToAsync(string id, int left, int top, ScrollBehavior scrollBehavior) => ValueTask.CompletedTask;

    public ValueTask ScrollIntoViewAsync(string selector, ScrollBehavior behavior) => ValueTask.CompletedTask;

    public Task ScrollToFragment(string id, ScrollBehavior behavior) => Task.CompletedTask;

    public ValueTask ScrollToFragmentAsync(string id, ScrollBehavior behavior) => ValueTask.CompletedTask;

    public ValueTask ScrollToListItemAsync(string elementId) => ValueTask.CompletedTask;

    public Task ScrollToTop(ScrollBehavior scrollBehavior = ScrollBehavior.Auto) => Task.CompletedTask;

    public ValueTask ScrollToTopAsync(string id, ScrollBehavior scrollBehavior = ScrollBehavior.Auto) => ValueTask.CompletedTask;

    public ValueTask ScrollToBottomAsync(string id, ScrollBehavior scrollBehavior = ScrollBehavior.Auto) => ValueTask.CompletedTask;

    public ValueTask ScrollToYearAsync(string elementId) => ValueTask.CompletedTask;

    public ValueTask UnlockScrollAsync(string elementId, string cssClass) => ValueTask.CompletedTask;
}

public class MockBrowserWindowSizeProvider : IBrowserWindowSizeProvider
{
    public ValueTask<BrowserWindowSize> GetBrowserWindowSize()
    {
        return new ValueTask<BrowserWindowSize>(new BrowserWindowSize());
    }
}

public class MockPopoverService : IMudPopoverService
{
    private List<MudPopoverHandler> _handlers = new();
    private static RenderFragment DefaultFragment = (builder) => { };
    public IEnumerable<MudPopoverHandler> Handlers => _handlers;

    public bool ThrowOnDuplicateProvider => false;

    public event EventHandler FragmentsChanged;
    public Task InitializeIfNeeded() => Task.FromResult(true);

    public MudPopoverHandler Register(RenderFragment fragment) => new MudPopoverHandler(fragment ?? DefaultFragment, Mock.Of<IJSRuntime>(), () =>
    {
        FragmentsChanged?.Invoke(this, EventArgs.Empty);
    });

    public Task<bool> Unregister(MudPopoverHandler handler) => Task.FromResult(true);

    public ValueTask<int> CountProviders() => ValueTask.FromResult(0);
}

public class MockPopoverServiceV2 : IPopoverService
{
    public PopoverOptions PopoverOptions { get; } = new();

    public IEnumerable<IMudPopoverHolder> ActivePopovers { get; } = Enumerable.Empty<IMudPopoverHolder>();

    public bool IsInitialized => false;

    public void Subscribe(IPopoverObserver observer)
    {
    }

    public void Unsubscribe(IPopoverObserver observer)
    {
    }

    public Task CreatePopoverAsync(IPopover mudPopover) => Task.CompletedTask;

    public Task<bool> UpdatePopoverAsync(IPopover mudPopover) => Task.FromResult(true);

    public Task<bool> DestroyPopoverAsync(IPopover mudPopover) => Task.FromResult(true);

    public ValueTask<int> GetProviderCountAsync() => ValueTask.FromResult(0);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}