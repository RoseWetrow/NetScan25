using System;
using System.Collections.Generic;
using System.Text;
using NetScan.Models;

namespace NetScan.Services
{
    /// Интерфейс простого сервиса работы с SQLite.
    public interface IDatabaseService
    {
        void CreateTable();
        void AddDevices(IEnumerable<Device> devices);
        List<Device> GetDevices();
        void DeleteAllDevices();
    }
}

