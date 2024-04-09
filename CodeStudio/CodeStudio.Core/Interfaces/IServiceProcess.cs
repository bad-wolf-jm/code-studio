using System.Threading.Tasks;

namespace Metrino.Development.UI.Core;

public interface IServiceProcess
{
    void Start();
    void Stop();

    Task StartAsync();
    Task StopAsync();
}
