using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NetScan.Models;

namespace NetScan.Services
{
    public class NetworkScannerService : INetworkScanner
    {
        /// Асинхронно перебирает возможные адреса в указанной подсети (x.x.x.1–254), сначала выполняет ping, при ответе — пытается разрешить имя через DNS. Прекращает сканирование при срабатывании CancellationToken.
        public async Task ScanSubnetAsync(string subnet, IProgress<Device> progress, CancellationToken ct, int maxConcurrency = 30)
        {
            var tasks = new List<Task>();
            var sem = new SemaphoreSlim(maxConcurrency);

            for (int i = 1; i < 255; i++)
            {
                if (ct.IsCancellationRequested) break;
                var ip = $"{subnet}.{i}";

                await sem.WaitAsync(ct);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var reply = await ping.SendPingAsync(ip, 300);
                            if (reply.Status == IPStatus.Success)
                            {
                                string hostname = "Unknown";
                                try
                                {
                                    var entry = await Dns.GetHostEntryAsync(ip);
                                    if (!string.IsNullOrWhiteSpace(entry.HostName) && entry.HostName != ip)
                                        hostname = entry.HostName;
                                }
                                catch
                                {
                                    // игнорируем DNS-ошибки
                                }

                                var device = new Device{Hostname = hostname, Ip = ip, ScanTime = DateTime.Now}; // добавление найденных параметров устройства в экземпляр модели
                                progress?.Report(device);                                                       // отправляем через progress (ViewModel добавит в коллекцию - отобразит)
                            }
                        }
                    }
                    catch (OperationCanceledException) { /* отмена */ }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, ct));
            }
            await Task.WhenAll(tasks);
        }
    }
}

