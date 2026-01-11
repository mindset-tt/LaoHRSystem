using System;
using System.Windows;
using System.Windows.Media;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.Bridge.Config
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Server=localhost;Database=LaoHR_DB;Trusted_Connection=True;TrustServerCertificate=True;";

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private async void LoadSettings()
        {
            try
            {
                TxtStatus.Text = "Loading settings...";
                var options = new DbContextOptionsBuilder<LaoHRDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

                using var db = new LaoHRDbContext(options);
                
                var enabled = await db.SystemSettings.FindAsync("ZKTECO_ENABLED");
                var ip = await db.SystemSettings.FindAsync("ZK_DEVICE_IP");
                var port = await db.SystemSettings.FindAsync("ZK_DEVICE_PORT");

                if (enabled != null && bool.TryParse(enabled.SettingValue, out bool isEnabled))
                    ChkEnabled.IsChecked = isEnabled;

                if (ip != null) TxtIp.Text = ip.SettingValue;
                if (port != null) TxtPort.Text = port.SettingValue;

                TxtStatus.Text = "Settings loaded.";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Error: {ex.Message}";
                TxtStatus.Foreground = Brushes.Red;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnSave.IsEnabled = false;
                TxtStatus.Text = "Saving...";

                var options = new DbContextOptionsBuilder<LaoHRDbContext>()
                    .UseSqlServer(ConnectionString)
                    .Options;

                using var db = new LaoHRDbContext(options);

                await UpdateSetting(db, "ZKTECO_ENABLED", ChkEnabled.IsChecked == true ? "true" : "false");
                await UpdateSetting(db, "ZK_DEVICE_IP", TxtIp.Text);
                await UpdateSetting(db, "ZK_DEVICE_PORT", TxtPort.Text);

                await db.SaveChangesAsync();
                
                TxtStatus.Text = "Settings saved successfully. The Bridge Service will pick up changes in 10s.";
                TxtStatus.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"Error saving: {ex.Message}";
                TxtStatus.Foreground = Brushes.Red;
            }
            finally
            {
                BtnSave.IsEnabled = true;
            }
        }

        private async System.Threading.Tasks.Task UpdateSetting(LaoHRDbContext db, string key, string value)
        {
            var s = await db.SystemSettings.FindAsync(key);
            if (s == null)
            {
                db.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value });
            }
            else
            {
                s.SettingValue = value;
            }
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            string ip = TxtIp.Text;
            int port = int.TryParse(TxtPort.Text, out int p) ? p : 4370;

            TxtStatus.Text = $"Connecting to {ip}:{port}...";
            TxtStatus.Foreground = Brushes.Black;
            BtnTest.IsEnabled = false;

            await System.Threading.Tasks.Task.Run(() =>
            {
                using var device = new ZkDevice("Test", ip, port);
                if (device.Connect())
                {
                    Dispatcher.Invoke(() => 
                    {
                        TxtStatus.Text = $"Success! Connected to S/N: {device.SerialNumber}";
                        TxtStatus.Foreground = Brushes.Green;
                    });
                }
                else
                {
                     Dispatcher.Invoke(() => 
                    {
                        TxtStatus.Text = "Connection Failed. Check IP/Port and ensure device is reachable (and zkemkeeper.dll is registered).";
                        TxtStatus.Foreground = Brushes.Red;
                    });
                }
            });
            
            BtnTest.IsEnabled = true;
        }
    }
}