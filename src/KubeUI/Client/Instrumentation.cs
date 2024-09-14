using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrutor;

namespace KubeUI.Client;

[ServiceDescriptor<Instrumentation>(ServiceLifetime.Singleton)]
public class Instrumentation
{
    public Counter<long> ViewOpened { get; private set; }

    public Instrumentation(IMeterFactory meterFactory)
    {
        const string prefix = "kubeui";
        var meter = meterFactory.Create(prefix);

        ViewOpened = meter.CreateCounter<long>(prefix + "_view_opened", description: "View Opened");
    }
}
