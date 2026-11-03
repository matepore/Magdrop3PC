using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MagicalDropLauncher;

public partial class MainWindow : Window
{
    private static readonly string[] RequiredGameRoms =
    [
        "233-p1.bin",
        "233-c1.bin",
        "233-c2.bin",
        "233-c3.bin",
        "233-c4.bin",
        "233-s1.bin",
        "233-m1.bin",
        "233-v1.bin",
        "233-v2.bin"
    ];

    private static readonly string[] RequiredSystemRoms =
    [
        "sfix.sfix",
        "sm1.sm1",
        "000-lo.lo"
    ];

    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Magdrop3PC", "Launcher");

    private static readonly string SettingsFile =
        Path.Combine(SettingsDirectory, "launcher-settings.json");

    private readonly string _gameDirectory;
    private string _romDirectory;
    private string _biosDirectory;
    private string _executablePath;
    private bool _isRefreshing;

    public MainWindow()
    {
        InitializeComponent();

        string repositoryRoot = FindRepositoryRoot() ?? AppContext.BaseDirectory;
        _gameDirectory = Path.Combine(repositoryRoot, "magdrop3");

        LauncherSettings settings = LoadSettings();
        _romDirectory = settings.RomDirectory ?? Path.Combine(_gameDirectory, "roms");
        _biosDirectory = settings.BiosDirectory ?? Path.Combine(_gameDirectory, "bios");
        _executablePath = settings.ExecutablePath ??
            Path.Combine(_gameDirectory, "build-final", "magdrop3.exe");

        BiosRegionCombo.SelectedIndex = 0;
        UpdatePathLabels();
        RefreshValidation();
    }

    private static string? FindRepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);

        while (current is not null)
        {
            string gameDirectory = Path.Combine(current.FullName, "magdrop3");
            if (File.Exists(Path.Combine(gameDirectory, "CMakeLists.txt")))
                return current.FullName;

            current = current.Parent;
        }

        return null;
    }

    private static LauncherSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                return JsonSerializer.Deserialize<LauncherSettings>(
                    File.ReadAllText(SettingsFile)) ?? new LauncherSettings();
            }
        }
        catch (Exception)
        {
            // A damaged local preferences file must not prevent the launcher from opening.
        }

        return new LauncherSettings();
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            var settings = new LauncherSettings
            {
                RomDirectory = _romDirectory,
                BiosDirectory = _biosDirectory,
                ExecutablePath = _executablePath
            };

            File.WriteAllText(SettingsFile,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            StatusDetailText.Text = $"Could not save launcher preferences: {ex.Message}";
        }
    }

    private string SelectedBiosFile =>
        (BiosRegionCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "sp-s2.sp1";

    private void UpdatePathLabels()
    {
        RomDirectoryText.Text = _romDirectory;
        BiosDirectoryText.Text = _biosDirectory;
        ExecutableText.Text = _executablePath;
        RomDirectoryText.ToolTip = _romDirectory;
        BiosDirectoryText.ToolTip = _biosDirectory;
        ExecutableText.ToolTip = _executablePath;
    }

    private void RefreshValidation()
    {
        _isRefreshing = true;
        try
        {
            var issues = new List<string>();

            if (!Directory.Exists(_romDirectory))
            {
                issues.Add($"ROM directory not found: {_romDirectory}");
            }
            else
            {
                foreach (string file in RequiredGameRoms)
                {
                    if (!File.Exists(Path.Combine(_romDirectory, file)))
                        issues.Add($"Missing game ROM: {file}");
                }
            }

            if (!Directory.Exists(_biosDirectory))
            {
                issues.Add($"BIOS directory not found: {_biosDirectory}");
            }
            else
            {
                string selectedBios = Path.Combine(_biosDirectory, SelectedBiosFile);
                if (!File.Exists(selectedBios))
                    issues.Add($"Missing selected BIOS: {SelectedBiosFile}");

                foreach (string file in RequiredSystemRoms)
                {
                    if (!File.Exists(Path.Combine(_biosDirectory, file)))
                        issues.Add($"Missing system ROM: {file}");
                }
            }

            if (!File.Exists(_executablePath))
                issues.Add($"Game executable not found: {_executablePath}");

            ValidationList.ItemsSource = issues;
            LaunchButton.IsEnabled = issues.Count == 0;

            if (issues.Count == 0)
            {
                StatusText.Text = "ALL FILES READY";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                StatusDetailText.Text = "Required ROMs, BIOS and executable were found. You can start the game.";
            }
            else
            {
                StatusText.Text = $"{issues.Count} ISSUE(S) FOUND";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
                StatusDetailText.Text = "Resolve the items below before starting the game.";
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void BiosRegionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isRefreshing || !IsInitialized)
            return;

        RefreshValidation();
    }

    private void RefreshFiles_Click(object sender, RoutedEventArgs e)
    {
        UpdatePathLabels();
        RefreshValidation();
    }

    private void BrowseRomDirectory_Click(object sender, RoutedEventArgs e)
    {
        string? selected = BrowseForFolder("Select the Magical Drop III ROM directory", _romDirectory);
        if (selected is null) return;

        _romDirectory = selected;
        UpdatePathLabels();
        SaveSettings();
        RefreshValidation();
    }

    private void BrowseBiosDirectory_Click(object sender, RoutedEventArgs e)
    {
        string? selected = BrowseForFolder("Select the Neo Geo BIOS directory", _biosDirectory);
        if (selected is null) return;

        _biosDirectory = selected;
        UpdatePathLabels();
        SaveSettings();
        RefreshValidation();
    }

    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select the Magical Drop III executable",
            Filter = "Magical Drop III executable (magdrop3.exe)|magdrop3.exe|Executable files (*.exe)|*.exe",
            CheckFileExists = true,
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(_executablePath))
                ? Path.GetDirectoryName(_executablePath)
                : _gameDirectory
        };

        if (dialog.ShowDialog(this) != true) return;

        _executablePath = dialog.FileName;
        UpdatePathLabels();
        SaveSettings();
        RefreshValidation();
    }

    private string? BrowseForFolder(string title, string initialDirectory)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title,
            Multiselect = false,
            InitialDirectory = Directory.Exists(initialDirectory)
                ? initialDirectory
                : _gameDirectory
        };

        return dialog.ShowDialog(this) == true ? dialog.FolderName : null;
    }

    private void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        RefreshValidation();
        if (!LaunchButton.IsEnabled)
            return;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                WorkingDirectory = _gameDirectory,
                UseShellExecute = true
            };

            startInfo.ArgumentList.Add("--rom-path");
            startInfo.ArgumentList.Add(_romDirectory);
            startInfo.ArgumentList.Add("--bios-path");
            startInfo.ArgumentList.Add(_biosDirectory);
            startInfo.ArgumentList.Add("--bios");
            startInfo.ArgumentList.Add(SelectedBiosFile);

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"The game could not be started.\n\n{ex.Message}",
                "Magical Drop III - Launch error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private sealed class LauncherSettings
    {
        public string? RomDirectory { get; set; }
        public string? BiosDirectory { get; set; }
        public string? ExecutablePath { get; set; }
    }
}
