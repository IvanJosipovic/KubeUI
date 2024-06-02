using System;
using System.IO;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using k8s.Models;
using k8s;
using System.Threading.Tasks;
using Avalonia.Threading;
using KubeUI.Client;
using System.Net.WebSockets;
using Avalonia.Input;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace KubeUI.ViewModels;

public sealed partial class PodConsoleViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<PodConsoleViewModel> _logger;

    public PodConsoleViewModel(ILogger<PodConsoleViewModel> logger)
    {
        _logger = logger;
    }

    [ObservableProperty]
    Cluster _cluster;

    [ObservableProperty]
    V1Pod _object;

    [ObservableProperty]
    string _containerName;

    [ObservableProperty]
    TextDocument _console = new();

    WebSocket? _webSocket;
    StreamDemuxer? _streamDemuxer;
    Stream? _stream;
    StreamReader? _streamReader;

    public async Task Connect()
    {
        var command = new string[]
        {
            "sh",
            "-c",
            "clear; (bash || ash || sh)"
        };

        _webSocket = await Cluster.Client.WebSocketNamespacedPodExecAsync(Object.Name(), Object.Namespace(), command, ContainerName);

        _streamDemuxer = new StreamDemuxer(_webSocket);
        _streamDemuxer.Start();

        _stream = _streamDemuxer.GetStream(ChannelIndex.StdOut, ChannelIndex.StdIn);

        _streamReader = new StreamReader(_stream);

        _ = Task.Run(async () =>
        {
            while (_stream.CanRead)
            {
                try
                {
                    var memory = new Memory<char>(new char[1024]);
                    await _streamReader.ReadAsync(memory);
                    var str = memory.ToString().Replace("\0", "").Replace("\a", "").Replace("\b", "");
                    str = RemoveAnsiEscapeSequences(str);
                    if (!string.IsNullOrEmpty(str))
                    {
                       await Dispatcher.UIThread.InvokeAsync(() => Console.Insert(Console.TextLength, str));
                    }
                }
                catch (IOException ex) when (ex.Message.Equals("The request was aborted.")) { break; }
                catch (ObjectDisposedException) { break; }
            }
        });
    }

    public static string RemoveAnsiEscapeSequences(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // ANSI escape sequences pattern
        var ansiEscapeRegex = new Regex(@"\x1B\[[0-?]*[ -/]*[@-~]");
        return ansiEscapeRegex.Replace(text, string.Empty);
    }

    public void KeyUp(KeyEventArgs args)
    {
        if (_stream?.CanWrite == true)
        {
            try
            {
                if (!string.IsNullOrEmpty(args.KeySymbol))
                {
                    _stream.Write(Encoding.Unicode.GetBytes(args.KeySymbol));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending key to console: ");
            }
        }
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
        _streamDemuxer?.Dispose();
        _stream?.Dispose();
        _streamReader?.Dispose();
    }
}
