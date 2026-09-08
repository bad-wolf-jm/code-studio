using System.Threading.Tasks;

namespace UI.Core;

public interface IServiceProcess
{
    void Start();
    void Stop();

    Task StartAsync();
    Task StopAsync();
}
