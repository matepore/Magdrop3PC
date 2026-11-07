using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;

namespace MagicalDropLauncher;

internal sealed record DependencyStatus(string Key, string Name, bool Installed, string Details);

internal static class DependencyManager
{
    private static readonly HttpClient Http = new();

    public static async Task<IReadOnlyList<DependencyStatus>> CheckAsync(string repositoryRoot)
    {
        string? visualStudio = await FindVisualStudioInstallationAsync();
        string? nmake = FindNMake(visualStudio);
        string? cmake = FindCMake();
        string vcpkgRoot = Path.Combine(repositoryRoot, "vcpkg");
        string vcpkgExe = Path.Combine(vcpkgRoot, "vcpkg.exe");
        string toolchain = Path.Combine(vcpkgRoot, "scripts", "buildsystems", "vcpkg.cmake");
        string sdl2Config = Path.Combine(vcpkgRoot, "installed", "x64-windows", "share", "sdl2", "SDL2Config.cmake");

        return
        [
            new("msvc", "Visual Studio C++ Build Tools", visualStudio is not null,
                visualStudio ?? "Visual Studio with the x64 C++ toolchain was not found."),
            new("nmake", "NMake", nmake is not null,
                nmake ?? "NMake was not found in the Visual Studio C++ tools."),
            new("cmake", "CMake", cmake is not null,
                cmake ?? "CMake was not found in the standard installation paths or PATH."),
            new("vcpkg", "vcpkg", File.Exists(vcpkgExe) && File.Exists(toolchain),
                File.Exists(vcpkgExe) && File.Exists(toolchain) ? vcpkgRoot : "The vcpkg checkout or CMake toolchain is missing."),
            new("sdl2", "SDL2 (x64-windows)", File.Exists(sdl2Config),
                File.Exists(sdl2Config) ? sdl2Config : "SDL2 is not installed in vcpkg's x64-windows triplet.")
        ];
    }

    public static async Task InstallMissingAsync(
        string repositoryRoot,
        IReadOnlyCollection<DependencyStatus> missing)
    {
        bool needsMsvc = missing.Any(item => item.Key is "msvc" or "nmake");
        bool needsCMake = missing.Any(item => item.Key == "cmake");
        bool needsVcpkg = missing.Any(item => item.Key == "vcpkg");
        bool needsSdl2 = missing.Any(item => item.Key == "sdl2");

        if (needsMsvc)
        {
            string? installation = await FindVisualStudioInstallationAsync();
            string installer = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer", "vs_installer.exe");

            if (installation is not null && File.Exists(installer))
            {
                await RunInteractiveAsync(installer,
                    $"modify --installPath \"{installation}\" --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended --passive --wait --norestart",
                    "Visual Studio C++ Build Tools");
            }
            else
            {
                await RunWingetAsync(
                    "install --id Microsoft.VisualStudio.2022.BuildTools --exact --source winget --accept-package-agreements --accept-source-agreements --override \"--wait --passive --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended\"",
                    "Visual Studio Build Tools with the C++ workload");
            }
        }

        if (needsCMake)
        {
            await RunWingetAsync(
                "install --id Kitware.CMake --exact --source winget --accept-package-agreements --accept-source-agreements",
                "CMake");
        }

        string vcpkgRoot = Path.Combine(repositoryRoot, "vcpkg");
        if (needsVcpkg || needsSdl2)
        {
            if (!Directory.Exists(vcpkgRoot))
                await DownloadVcpkgAsync(vcpkgRoot);

            string bootstrap = Path.Combine(vcpkgRoot, "bootstrap-vcpkg.bat");
            string vcpkgExe = Path.Combine(vcpkgRoot, "vcpkg.exe");

            if (!File.Exists(vcpkgExe))
            {
                if (!File.Exists(bootstrap))
                    throw new InvalidOperationException("The vcpkg bootstrap script was not found after downloading vcpkg.");

                await RunInteractiveAsync(
                    Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                    $"/d /s /c \"\"{bootstrap}\" -disableMetrics\"",
                    "vcpkg bootstrap");
            }

            if (needsSdl2)
            {
                if (!File.Exists(vcpkgExe))
                    throw new InvalidOperationException("vcpkg.exe was not created. SDL2 cannot be installed.");

                await RunInteractiveAsync(vcpkgExe, "install sdl2:x64-windows", "SDL2 (x64-windows)");
            }
        }
    }

    private static async Task<string?> FindVisualStudioInstallationAsync()
    {
        string vswhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");

        if (!File.Exists(vswhere))
            return null;

        var result = await RunCapturedAsync(vswhere,
            "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath");

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Output))
            return null;

        return result.Output.Split(new[] { '\\r', '\\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim();
    }

    private static string? FindNMake(string? visualStudio)
    {
        if (string.IsNullOrWhiteSpace(visualStudio))
            return null;

        string msvcRoot = Path.Combine(visualStudio, "VC", "Tools", "MSVC");
        if (!Directory.Exists(msvcRoot))
            return null;

        foreach (string version in Directory.GetDirectories(msvcRoot).OrderByDescending(path => path))
        {
            string[] candidates =
            [
                Path.Combine(version, "bin", "Hostx64", "x64", "nmake.exe"),
                Path.Combine(version, "bin", "Hostx86", "x86", "nmake.exe"),
                Path.Combine(version, "bin", "Hostx64", "x86", "nmake.exe")
            ];

            string? found = candidates.FirstOrDefault(File.Exists);
            if (found is not null)
                return found;
        }

        return null;
    }

    private static string? FindCMake()
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string[] candidates =
        [
            Path.Combine(programFiles, "CMake", "bin", "cmake.exe"),
            Path.Combine(programFilesX86, "CMake", "bin", "cmake.exe")
        ];

        string? installed = candidates.FirstOrDefault(File.Exists);
        if (installed is not null)
            return installed;

        string? path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrWhiteSpace(path))
        {
            foreach (string directory in path.Split(Path.PathSeparator,
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string candidate = Path.Combine(directory.Trim('"'), "cmake.exe");
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
    }

    private static async Task DownloadVcpkgAsync(string destination)
    {
        string archivePath = Path.Combine(Path.GetTempPath(), $"vcpkg-{Guid.NewGuid():N}.zip");
        string extractionPath = Path.Combine(Path.GetTempPath(), $"vcpkg-{Guid.NewGuid():N}");

        try
        {
            byte[] archive = await Http.GetByteArrayAsync(
                "https://github.com/microsoft/vcpkg/archive/refs/heads/master.zip");
            await File.WriteAllBytesAsync(archivePath, archive);
            Directory.CreateDirectory(extractionPath);
            ZipFile.ExtractToDirectory(archivePath, extractionPath);

            string? extractedRoot = Directory.GetDirectories(extractionPath)
                .FirstOrDefault(path => File.Exists(Path.Combine(path, "bootstrap-vcpkg.bat")));

            if (extractedRoot is null)
                throw new InvalidOperationException("The downloaded vcpkg archive has an unexpected structure.");

            if (Directory.Exists(destination))
                Directory.Delete(destination, recursive: true);

            Directory.Move(extractedRoot, destination);
        }
        finally
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            if (Directory.Exists(extractionPath))
                Directory.Delete(extractionPath, recursive: true);
        }
    }

    private static async Task RunWingetAsync(string arguments, string displayName)
    {
        string winget = FindWinget()
            ?? throw new InvalidOperationException(
                $"Windows Package Manager (winget) is required to install {displayName}. Install or update 'App Installer' from Microsoft Store, then try again.");

        await RunInteractiveAsync(winget, arguments, displayName);
    }

    private static string? FindWinget()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string candidate = Path.Combine(localAppData, "Microsoft", "WindowsApps", "winget.exe");
        if (File.Exists(candidate))
            return candidate;

        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
            return null;

        foreach (string directory in path.Split(Path.PathSeparator,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            candidate = Path.Combine(directory.Trim('"'), "winget.exe");
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static async Task RunInteractiveAsync(string fileName, string arguments, string displayName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start the installer for {displayName}.");

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"The installation of {displayName} failed (exit code {process.ExitCode}).");
    }

    private static async Task<(int ExitCode, string Output)> RunCapturedAsync(
        string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start {fileName}.");

        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, (await stdout) + Environment.NewLine + (await stderr));
    }
}
