using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace NetScan.Models
{
    /// Класс модели данных для таблицы Device. Хранит идентификатор, название устройства, IP-адрес и время сканирования.
    public class Device
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// имя хоста (hostname), "Unknown" при отсутствии DNS-имени
        public string Hostname { get; set; }

        /// IP-адрес устройства
        public string Ip { get; set; }

        /// время проведения сканирования, заполняется при сохранении в БД
        public DateTime ScanTime { get; set; }
    }
}
