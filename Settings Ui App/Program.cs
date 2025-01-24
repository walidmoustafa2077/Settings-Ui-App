using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Settings_Ui_App
{
    internal class Program
    {
        static string settingsFilePath = "settings.json";
        static string currentVersion = "1.0.0";
        static string updateCheckUrl = "https://example.com/latest-version.json";
        static string updateDownloadUrl = "https://example.com/app-update.zip";
        static string updaterPath = "Updater.exe";

        static void Main(string[] args)
        {
            Console.WriteLine($"Welcome to the Settings UI Version {currentVersion}\n");

            // Check for updates
            // CheckForUpdates();

            // Load existing settings or create defaults
            var settings = LoadSettings();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n======= Settings Menu =======");
                Console.WriteLine("1. View Settings");
                Console.WriteLine("2. Update Settings");
                Console.WriteLine("3. Save Settings");
                Console.WriteLine("4. Exit");
                Console.WriteLine("=============================\n");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ViewSettings(settings);
                        break;
                    case "2":
                        UpdateSettings(settings);
                        break;
                    case "3":
                        SaveSettings(settings);
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("Exiting application. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.\n");
                        break;
                }
            }
        }

        static void CheckForUpdates()
        {
            try
            {
                using var httpClient = new HttpClient();
                Console.WriteLine("Checking for updates...");

                var response = httpClient.GetStringAsync(updateCheckUrl).Result;
                var latestVersionInfo = JsonSerializer.Deserialize<VersionInfo>(response);

                if (latestVersionInfo != null && latestVersionInfo.Version != currentVersion)
                {
                    Console.WriteLine($"A new version ({latestVersionInfo.Version}) is available.");
                    Console.Write("Do you want to update? (yes/no): ");
                    var userInput = Console.ReadLine();

                    if (userInput?.Trim().ToLower() == "yes")
                    {
                        DownloadAndStartUpdater(latestVersionInfo.DownloadUrl, latestVersionInfo.Version);
                    }
                }
                else
                {
                    Console.WriteLine("You are already using the latest version.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
            }
        }

        static void DownloadAndStartUpdater(string downloadUrl, string latestVersion)
        {
            try
            {
                string tempZipPath = Path.Combine(Path.GetTempPath(), "app-update.zip");

                using var httpClient = new HttpClient();
                Console.WriteLine("Downloading the update...");

                var response = httpClient.GetAsync(downloadUrl).Result;
                var updateFile = response.Content.ReadAsByteArrayAsync().Result;

                File.WriteAllBytes(tempZipPath, updateFile);
                Console.WriteLine("Update downloaded successfully.");

                // Launch the updater
                Console.WriteLine("Launching updater...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = $"\"{tempZipPath}\" \"{AppDomain.CurrentDomain.BaseDirectory}\" \"{Assembly.GetExecutingAssembly().Location}\"",
                    UseShellExecute = true,
                });

                Console.WriteLine("Exiting application to complete the update...");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating application: {ex.Message}");
            }
        }

        static Settings LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                var json = File.ReadAllText(settingsFilePath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }

            return new Settings();
        }

        static void ViewSettings(Settings settings)
        {
            Console.WriteLine("\nCurrent Settings:");
            Console.WriteLine($"Theme: {settings.Theme}");
            Console.WriteLine($"FontSize: {settings.FontSize}");
            Console.WriteLine($"NotificationsEnabled: {settings.NotificationsEnabled}\n");
        }

        static void UpdateSettings(Settings settings)
        {
            Console.Write("Enter Theme (Light/Dark): ");
            var theme = Console.ReadLine();
            if (!string.IsNullOrEmpty(theme)) settings.Theme = theme;

            Console.Write("Enter Font Size (e.g., 12, 14, 16): ");
            if (int.TryParse(Console.ReadLine(), out int fontSize)) settings.FontSize = fontSize;

            Console.Write("Enable Notifications (true/false): ");
            if (bool.TryParse(Console.ReadLine(), out bool notificationsEnabled)) settings.NotificationsEnabled = notificationsEnabled;

            Console.WriteLine("Settings updated!\n");
        }

        static void SaveSettings(Settings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
            Console.WriteLine("Settings saved to 'settings.json'.\n");
        }
    }

    class Settings
    {
        public string Theme { get; set; } = "Light";
        public int FontSize { get; set; } = 12;
        public bool NotificationsEnabled { get; set; } = true;
    }

    class VersionInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
    }
}
