using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NetScan.Views
{
    /// Страница сканирования локальной сети. После нажатия на кнопку «Сканировать»
    /// определяется подсеть текущего интерфейса, происходит обход адресов
    /// от 1 до 254, для каждого IP осуществляется DNS‑разрешение и пинг. Устройства
    /// отображаются на экране в виде фреймов и могут быть сохранены в SQLite‑базу.
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {
        // Флаг, указывающий активен ли процесс сканирования. Позволяет остановить
        // вывод устройств после сохранения.
        private bool isScanning = true;

        // Список обнаруженных устройств до момента сохранения. Каждый элемент
        // содержит hostname и ip, а также id и время сканирования при записи в БД.
        private List<Device> deviceList;

        // Кнопка сохранения, создаётся при первом сканировании, затем используется
        // повторно, чтобы не добавлять несколько кнопок в интерфейс.
        private Xamarin.Forms.Button save;

        // Признак существования таблицы Device в базе. Таблица создаётся один
        // раз при первом открытии страницы.
        private bool tableExist = false;

        public ScanPage()
        {
            InitializeComponent();
        }


        /// Вызывается при отображении страницы. Создаёт таблицу для хранения
        /// устройств при первом открытии.
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!tableExist)
            {
                CreateTable();
                tableExist = true;
            }
        }


        /// Обработчик нажатия на кнопку «Сканировать». Сбрасывает интерфейс,
        /// подготавливает список устройств и запускает сканирование. Если кнопка
        /// сохранения ещё не создана – создаёт её и добавляет в контейнер.
        private void Button_Scan(object sender, EventArgs e)
        {
            Console.WriteLine("Сработал обработчик Button_Scan !");

            // активируем флаг сканирования и очищаем список устройств
            isScanning = true;
            deviceList = new List<Device>();

            // обновляем внешний вид кнопки сканирования
            scan.Text = string.Empty;
            scan.ImageSource = "button_repeat.ico";
            scan.WidthRequest = 105;
            scan.HeightRequest = 47;

            // очищаем список и информационную строку перед новым сканированием
            list.Children.Clear();
            info.Text = string.Empty;

            // определяем подсеть и начинаем сканирование
            GetLocalIP();

            // создаём кнопку сохранения, если она ещё не была создана
            if (save == null)
            {
                save = new Xamarin.Forms.Button
                {
                    Text = "Сохранить",
                    FontSize = 10,
                    Padding = new Thickness(25, 15),
                    CornerRadius = 50,
                };
                save.Clicked += Button_Save;

                // контейнер для кнопок объявлен в разметке под именем «buttons»
                buttons.Children.Add(save);
            }
        }


        /// Обработчик кнопки сохранения. Останавливает сканирование, перебирает
        /// обнаруженные устройства и заносит каждое в базу. После записи список очищается.
        private void Button_Save(object sender, EventArgs e)
        {
            Console.WriteLine("Сработал обработчик Button_Save !");

            // прекращаем вывод новых устройств
            isScanning = false;

            if (deviceList != null)
            {
                foreach (Device device in deviceList)
                {
                    AddDevice(device);
                    Console.WriteLine($"В базу успешно добавлена информация: {device.ip}");
                }
                // обнуляем список, чтобы избежать повторного добавления
                deviceList = null;
            }
        }


        /// Получает подсеть локального IP‑адреса и запускает сканирование. Метод
        /// исключает loopback (127.0.0) и выводит сетевой префикс на экран.
        private void GetLocalIP()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in interfaces)
            {
                var properties = adapter.GetIPProperties();
                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        var bytes = unicast.Address.GetAddressBytes();
                        var subnet = $"{bytes[0]}.{bytes[1]}.{bytes[2]}";
                        if (subnet != "127.0.0")
                        {
                            Console.WriteLine($"Полученный IP‑адрес из метода GetLocalIP: {subnet}");
                            title.Text = $"Сканируемая сеть: {subnet}";
                            Scan(subnet);
                            return;
                        }
                        else
                        {
                            Console.WriteLine($"IP‑адрес из метода GetLocalIP посчитался некорректным: {subnet}");
                        }
                    }
                }
            }
        }


        /// Асинхронно перебирает все возможные адреса в указанной подсети (x.x.x.1–254),
        /// определяет имя хоста через DNS и отправляет IP в метод PingIP().
        /// Сканирование прекращается, если флаг isScanning становится false.
        private async void Scan(string subnet)
        {
            Console.WriteLine("Запущен метод Scan();");
            if (!isScanning)
                return;

            for (int i = 1; i < 255; i++)
            {
                var ipAddress = $"{subnet}.{i}";
                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
                    var hostname = hostEntry.HostName;
                    // если DNS‑имя совпадает с IP, считаем, что имя неизвестно
                    if (ipAddress == hostname)
                    {
                        hostname = "Unknown";
                    }
                    PingIP(hostname, ipAddress);
                }
                catch
                {
                    // игнорируем DNS‑ошибки
                    PingIP("Unknown", ipAddress);
                }
            }
        }


        /// Асинхронно отправляет ping по указанному адресу. Если устройство отвечает
        /// за время таймаута, выводит его на экран через Print().
        private async void PingIP(string hostname, string ipAddress)
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, 100);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine($"Устройство с hostname: {hostname} и ip: {ipAddress} активно!");
                Print(hostname, ipAddress);
            }
        }


        /// Создаёт визуальный фрейм для обнаруженного устройства и добавляет его
        /// в StackLayout. Записывает устройство во временный список для дальнейшего
        /// сохранения.
        private void Print(string hostname, string ipAddress)
        {
            if (!isScanning)
                return;

            Console.WriteLine("Запущен метод Print();");
            // контейнер list определён в XAML
            if (list == null)
                return;

            // фрейм для устройства
            Frame frame = new Frame
            {
                AutomationId = "frameInStack",
                Margin = 3
            };

            // основной горизонтальный контейнер
            StackLayout stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15
            };

            // индикатор доступности устройства
            Xamarin.Forms.BoxView boxView = new Xamarin.Forms.BoxView
            {
                AutomationId = "boxViewInStack",
                CornerRadius = 100,
                Color = Color.Green
            };

            // вертикальный контейнер для текста
            StackLayout stackLayout2 = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 1
            };

            Label labelHost = new Label
            {
                FontAttributes = FontAttributes.Bold,
                Text = hostname
            };
            // корректируем размер шрифта для длинных имён
            labelHost.FontSize = (labelHost.Text?.Length >= 27) ? 13 : 14;

            Label labelIP = new Label
            {
                Text = ipAddress,
                FontSize = 14
            };

            // добавляем обработчик нажатия для отображения информации
            frame.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    Info(hostname, ipAddress);
                })
            });

            // формируем иерархию элементов
            stackLayout.Children.Add(boxView);
            stackLayout.Children.Add(stackLayout2);
            stackLayout2.Children.Add(labelHost);
            stackLayout2.Children.Add(labelIP);
            frame.Content = stackLayout;
            list.Children.Add(frame);

            // добавляем запись в список для сохранения
            Device device = new Device
            {
                hostname = hostname,
                ip = ipAddress
            };
            deviceList.Add(device);
        }


        /// Отображает всплывающее окно с информацией об устройстве. Для
        /// упрощения всегда предполагается, что устройство активно, так как
        /// вызывается из успешного ответа ping.
        public async void Info(string hostname, string ipAddress)
        {
            string info = (hostname != "Unknown" ? $"Устройство : {hostname}" : "Неудалось получить название устройства") +
                          $"\nIP‑адрес: {ipAddress}\nУстройство активно";
            await DisplayAlert("Информация", info, "OK");
        }

        #region БД и её методы


        /// Класс модели данных для таблицы Device. Хранит идентификатор,
        /// название устройства, IP‑адрес и время сканирования.
        public class Device
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string hostname { get; set; }
            public string ip { get; set; }
            public DateTime scanTime { get; set; }
        }


        /// Метод подключения к базе данных. Создаёт файл netscan.db3 в
        /// папке Personal, если его ещё нет, и возвращает соединение.
        public static SQLiteConnection GetConnection()
        {
            string dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "netscan.db3");
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            return connection;
        }


        /// Создаёт таблицу Device в БД, если она ещё не существует.
        public void CreateTable()
        {
            using (SQLiteConnection connection = GetConnection())
            {
                connection.CreateTable<Device>();
            }
        }


        /// Добавляет устройство в БД, заполняя время сканирования. Таблица
        /// создаётся при необходимости.
        public void AddDevice(Device info)
        {
            using (SQLiteConnection connection = GetConnection())
            {
                // создаём таблицу, если её нет
                connection.CreateTable<Device>();
                Device device = new Device
                {
                    hostname = info.hostname,
                    ip = info.ip,
                    scanTime = DateTime.Now
                };
                connection.Insert(device);
            }
        }


        /// Полностью очищает историю, удаляя таблицу Device и создавая её заново.
        /// Статический метод необходим для вызова из других классов.
        public static void DeleteAllHistory()
        {
            using (SQLiteConnection connection = GetConnection())
            {
                connection.DropTable<Device>();
                connection.CreateTable<Device>();
            }
        }
        #endregion
    }
}