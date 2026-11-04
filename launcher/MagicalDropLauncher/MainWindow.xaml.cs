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

    private string SelectedBuildRegion =>
        SelectedBiosFile switch
        {
            "sp-u2.sp1" => "USA",
            "sp-j2.sp1" => "Japan",
            _ => "Europe-Asia"
        };

    private void UpdatePathLabels()
    {
        RomDirectoryText.Text = _romDirectory;
        BiosDirectoryText.Text = _biosDirectory;
        ExecutableText.Text = _executablePath;
        RomDirectoryText.ToolTip = _romDirectory;
        BiosDirectoryText.ToolTip = _biosDirectory;
        ExecutableText.ToolTip = _executablePath;
    }

    private List<string> GetDataValidationIssues()
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
            if (!File.Exists(Path.Combine(_biosDirectory, SelectedBiosFile)))
                issues.Add($"Missing selected BIOS: {SelectedBiosFile}");

            foreach (string file in RequiredSystemRoms)
            {
                if (!File.Exists(Path.Combine(_biosDirectory, file)))
                    issues.Add($"Missing system ROM: {file}");
            }
        }

        return issues;
    }

    private void RefreshValidation()
    {
        _isRefreshing = true;
        try
        {
            List<string> issues = GetDataValidationIssues();
            if (!File.Exists(_executablePath))
                issues.Add($"Game executable not found: {_executablePath}");

            ValidationList.ItemsSource = issues;
            LaunchButton.IsEnabled = issues.Count == 0 && BuildButton.IsEnabled;

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

    private static string? FindVcpkgToolchain(string startDirectory)
    {
        DirectoryInfo? current = new(startDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(current.FullName, "vcpkg", "scripts",
                "buildsystems", "vcpkg.cmake");
            if (File.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        return null;
    }

    private static string? FindExecutableInPath(string executableName)
    {
        string? pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
            return null;

        foreach (string directory in pathValue.Split(Path.PathSeparator,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string candidate = Path.Combine(directory.Trim('"'), executableName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static async Task<(int ExitCode, string Output)> RunCommandAsync(
        string fileName, IEnumerable<string> arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        string output = await stdoutTask + Environment.NewLine + await stderrTask;
        return (process.ExitCode, output.Trim());
    }

    private async void BuildGame_Click(object sender, RoutedEventArgs e)
    {
        List<string> issues = GetDataValidationIssues();
        if (issues.Count > 0)
        {
            ValidationList.ItemsSource = issues;
            StatusText.Text = $"{issues.Count} BUILD REQUIREMENT(S) MISSING";
            StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            StatusDetailText.Text = "Provide all required ROM and BIOS files before building.";
            return;
        }

        string cmake = "cmake";
        string buildDirectory = Path.Combine(_gameDirectory, $"Build-{SelectedBuildRegion}-NMake");
        string? toolchain = FindVcpkgToolchain(Directory.GetParent(_gameDirectory)?.FullName ?? _gameDirectory);

        // NMake is a single-configuration generator and requires an initialized MSVC environment.
        string? nmakePath = FindExecutableInPath("nmake.exe");
        string? compilerPath = FindExecutableInPath("cl.exe");
        string? targetArchitecture = Environment.GetEnvironmentVariable("VSCMD_ARG_TGT_ARCH");
        if (nmakePath is null || compilerPath is null ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("INCLUDE")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LIB")) ||
            (!string.IsNullOrWhiteSpace(targetArchitecture) &&
             !string.Equals(targetArchitecture, "x64", StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show(this,
                "NMake builds require the x64 MSVC developer environment.\n\n" +
                "Open the \"x64 Native Tools Command Prompt for VS 2026\" (or the equivalent " +
                "Developer Command Prompt for your Visual Studio version), then launch MagicalDropLauncher from it.\n\n" +
                "The launcher needs nmake.exe, cl.exe, INCLUDE and LIB to be available.",
                "Magical Drop III - Build environment",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        bool buildSucceeded = false;
        string? buildOutputExecutable = null;
        string? buildFailureMessage = null;

        BuildButton.IsEnabled = false;
        LaunchButton.IsEnabled = false;
        BiosRegionCombo.IsEnabled = false;
        StatusText.Text = "CONFIGURING BUILD";
        StatusText.Foreground = System.Windows.Media.Brushes.Orange;
        StatusDetailText.Text = $"Preparing {SelectedBuildRegion} build in {buildDirectory}...";
        ValidationList.ItemsSource = new[] { $"Build output: {buildDirectory}", "CMake configure: in progress" };

        try
        {
            var configureArgs = new List<string>
            {
                "-S", _gameDirectory,
                "-B", buildDirectory,
                "-G", "NMake Makefiles",
                "-DCMAKE_BUILD_TYPE=Release",
                $"-DMAGDROP3_ROM_DIR={_romDirectory}",
                $"-DMAGDROP3_BIOS_DIR={_biosDirectory}",
                $"-DMAGDROP3_BIOS_FILE={SelectedBiosFile}"
            };

            if (toolchain is not null)
            {
                configureArgs.Add($"-DCMAKE_TOOLCHAIN_FILE={toolchain}");
                configureArgs.Add("-DVCPKG_TARGET_TRIPLET=x64-windows");
            }

            var configure = await RunCommandAsync(cmake, configureArgs, _gameDirectory);
            if (configure.ExitCode != 0)
                throw new InvalidOperationException($"CMake configuration failed (exit code {configure.ExitCode}).\n\n{configure.Output}");

            StatusText.Text = "BUILDING MAGICAL DROP III";
            StatusDetailText.Text = $"Compiling {SelectedBuildRegion} configuration...";
            ValidationList.ItemsSource = new[]
            {
                $"Build output: {buildDirectory}",
                "CMake configure: completed",
                "CMake build: in progress"
            };

            var build = await RunCommandAsync(cmake,
                new[] { "--build", buildDirectory, "--target", "magdrop3" },
                _gameDirectory);

            if (build.ExitCode != 0)
                throw new InvalidOperationException($"Build failed (exit code {build.ExitCode}).\n\n{build.Output}");

            string[] executableCandidates =
            [
                Path.Combine(buildDirectory, "magdrop3.exe"),
                Path.Combine(buildDirectory, "Release", "magdrop3.exe")
            ];

            string? builtExecutable = executableCandidates.FirstOrDefault(File.Exists);
            if (builtExecutable is not null)
            {
                _executablePath = builtExecutable;
                UpdatePathLabels();
                SaveSettings();
            }

            buildOutputExecutable = builtExecutable;
            buildSucceeded = true;

            StatusText.Text = "BUILD COMPLETED";
            StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            StatusDetailText.Text = builtExecutable is null
                ? $"Build succeeded. Check the output directory: {buildDirectory}"
                : $"Executable ready: {builtExecutable}";
            ValidationList.ItemsSource = new[]
            {
                $"Region: {SelectedBuildRegion}",
                $"Output: {buildDirectory}",
                "CMake configure: completed",
                "CMake build: completed"
            };
        }
        catch (Exception ex)
        {
            buildFailureMessage = ex.Message;
            StatusText.Text = "BUILD FAILED";
            StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
            StatusDetailText.Text = ex.Message.Length > 900
                ? ex.Message[..900] + "..."
                : ex.Message;
            ValidationList.ItemsSource = new[]
            {
                $"Build output: {buildDirectory}",
                "See the status details for the CMake error."
            };
            MessageBox.Show(this, ex.Message, "Magical Drop III - Build error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            BuildButton.IsEnabled = true;
            BiosRegionCombo.IsEnabled = true;
            RefreshValidation();

            if (buildSucceeded)
            {
                StatusText.Text = "BUILD COMPLETED";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                StatusDetailText.Text = buildOutputExecutable is not null
                    ? $"Executable ready: {buildOutputExecutable}"
                    : $"Build succeeded. Check the output directory: {buildDirectory}";
            }
            else if (buildFailureMessage is not null)
            {
                StatusText.Text = "BUILD FAILED";
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                StatusDetailText.Text = buildFailureMessage.Length > 900
                    ? buildFailureMessage[..900] + "..."
                    : buildFailureMessage;
            }
        }
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
