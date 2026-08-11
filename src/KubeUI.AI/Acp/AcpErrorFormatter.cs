using dotacp.protocol;
using Newtonsoft.Json;
using StreamJsonRpc;

namespace KubeUI.AI.Acp;

internal static class AcpErrorFormatter
{
    public static bool IsAcpError(Exception exception)
        => FindRpcException(exception) is RemoteRpcException or AcpException;

    public static string Format(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var rpcException = FindRpcException(exception);
        if (rpcException is null)
            return exception.Message;

        int code;
        string message;
        object? data;
        switch (rpcException)
        {
            case AcpException acpException:
                code = acpException.Code;
                message = acpException.Message;
                data = acpException.ErrorData;
                break;
            case RemoteRpcException remoteException:
                code = (int)remoteException.ErrorCode.GetValueOrDefault();
                message = remoteException.Message ?? string.Empty;
                data = remoteException.DeserializedErrorData ?? remoteException.ErrorData;
                break;
            default:
                throw new InvalidOperationException("Unsupported ACP exception.");
        }

        var details = data is null
            ? string.Empty
            : $" Details: {JsonConvert.SerializeObject(data)}";
        return $"ACP error {code}: {message}.{details}";
    }

    private static Exception? FindRpcException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is AcpException acpException)
                return acpException;
            if (current is RemoteRpcException remoteException && remoteException.ErrorCode.HasValue)
                return remoteException;
        }

        if (exception is AggregateException aggregate)
            return aggregate.Flatten().InnerExceptions.Select(FindRpcException).FirstOrDefault(found => found is not null);

        return null;
    }
}
