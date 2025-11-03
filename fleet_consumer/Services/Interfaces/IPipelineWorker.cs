using System;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Interfaces;

public interface IPipelineWorker : IDisposable
{
    Task ChannelLoop(CancellationToken cancellationToken);
    Task EnqueueIfNewAsync(string path, CancellationToken cancellationToken);
}