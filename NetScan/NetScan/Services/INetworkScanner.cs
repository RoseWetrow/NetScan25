using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetScan.Models;

namespace NetScan.Services
{
    /// Интерфейс простого сетевого сканера.
    /// Метод ScanSubnetAsync асинхронно сканирует подсеть и сообщает найденные устройства через IProgress.
    public interface INetworkScanner
    {
        Task ScanSubnetAsync(string subnet, IProgress<Device> progress, CancellationToken ct, int maxConcurrency = 30);
    }
}

