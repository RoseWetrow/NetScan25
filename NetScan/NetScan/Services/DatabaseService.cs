using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SQLite;
using NetScan.Models;

namespace NetScan.Services
{
    /// Сервис подключения и операций с базой данных SQLite.
    /// Содержит методы CreateTable, AddDevices, GetDevices, DeleteAllDevices.
    /// Комментарии и поведение основаны на оригинальной реализации в ScanPage.
    public class DatabaseService : IDatabaseService
    {
        private readonly string dbPath;

        public DatabaseService()
        {
            // Создаёт файл netscan.db3 в папке Personal, если его ещё нет.
            dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "netscan.db3");
            CreateTable();
        }

        /// Метод подключения к базе данных. Создаёт файл netscan.db3 в
        /// папке Personal, если его ещё нет, и возвращает соединение.
        private SQLiteConnection GetConnection()
        {
            var connection = new SQLiteConnection(dbPath);
            return connection;
        }

        /// Создаёт таблицу Device в БД, если она ещё не существует.
        public void CreateTable()
        {
            using (var connection = GetConnection())
            {
                connection.CreateTable<Device>();
            }
        }

        /// Добавляет устройства в БД в составе транзакции, заполняя время сканирования.
        /// Логика и комментарии перенесены из оригинала.
        public void AddDevices(IEnumerable<Device> devices)
        {
            using (var connection = GetConnection())
            {
                connection.RunInTransaction(() =>
                {
                    foreach (var d in devices)
                    {
                        // создаём таблицу, если её нет
                        connection.CreateTable<Device>();

                        // создаём устройство для записи, проставляем время сканирования
                        var deviceToInsert = new Device
                        {
                            Hostname = d.Hostname,
                            Ip = d.Ip,
                            ScanTime = DateTime.Now
                        };

                        connection.Insert(deviceToInsert);
                    }
                });
            }
        }

        /// Читает все записи из таблицы Device, сортируя по убыванию Id,
        /// чтобы новые записи отображались первыми.
        public List<Device> GetDevices()
        {
            using (var connection = GetConnection())
            {
                var devices = connection.Query<Device>("SELECT * FROM Device ORDER BY Id DESC");
                return devices;
            }
        }

        /// Полностью очищает историю, удаляя таблицу Device и создавая её заново.
        public void DeleteAllDevices()
        {
            using (var connection = GetConnection())
            {
                connection.DropTable<Device>();
                connection.CreateTable<Device>();
            }
        }
    }
}
