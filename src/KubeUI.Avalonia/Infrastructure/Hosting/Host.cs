using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace KubeUI.Avalonia.Infrastructure.Hosting;

internal class Host : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => CancellationToken.None;

    public CancellationToken ApplicationStopping => CancellationToken.None;

    public CancellationToken ApplicationStopped => CancellationToken.None;

    public void StopApplication()
    {
    }
}
