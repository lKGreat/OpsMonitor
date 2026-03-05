using System.Threading.Channels;

namespace OpsMonitor.Api.Services;

public record ProbeDispatch(long MonitorId);

public interface IProbeDispatchQueue
{
    bool Enqueue(ProbeDispatch dispatch);
    ValueTask<ProbeDispatch> DequeueAsync(CancellationToken ct);
}

public class ProbeDispatchQueue : IProbeDispatchQueue
{
    private readonly Channel<ProbeDispatch> _channel = Channel.CreateUnbounded<ProbeDispatch>();

    public bool Enqueue(ProbeDispatch dispatch) => _channel.Writer.TryWrite(dispatch);

    public ValueTask<ProbeDispatch> DequeueAsync(CancellationToken ct) => _channel.Reader.ReadAsync(ct);
}
