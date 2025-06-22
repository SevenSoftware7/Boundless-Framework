using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Build.Locator;
using SevenDev.Boundless.Modding;
using SevenDev.Boundless.Utility;

internal partial class Program {

	[GeneratedRegex(@"^project/assembly_name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectAssemblyRegex();

	[GeneratedRegex(@"^config/name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectNameRegex();

	[GeneratedRegex(@"<Authors>(.*?)<\/Authors>", RegexOptions.IgnoreCase)]
	private static partial Regex CsprojAuthorRegex();

	[GeneratedRegex(@"<Version>(.*?)<\/Version>", RegexOptions.IgnoreCase)]
	private static partial Regex CsprojVersionRegex();


	private static async Task<int> Main(string[] args) {
		Option<FileInfo[]> projectsOption = new("--projects", "List of Godot project.godot files");
		Option<FileInfo?> godotExeOption = new("--godot", getDefaultValue: () => null, description: "Path to the Godot executable");
		Option<DirectoryInfo> outputOption = new("--output", getDefaultValue: () => new("./mod/"), description: "Output folder for .dll and .pck files");
		Option<string?> authorOption = new("--author", getDefaultValue: () => null, description: "Author name to include in the mod manifest");
		Option<string?> versionOption = new("--verison", getDefaultValue: () => null, description: "Version to include in the mod manifest");

		RootCommand? rootCommand = new("Godot Mod Packager") {
			projectsOption,
			godotExeOption,
			outputOption,
			authorOption,
			versionOption
		};

		rootCommand.SetHandler(Execute, projectsOption, godotExeOption, outputOption, authorOption, versionOption);

		return await rootCommand.InvokeAsync(args);
	}

	private static void Execute(FileInfo[] godotProjects, FileInfo? godotExe, DirectoryInfo output, string? author, string? version) {
		MSBuildLocator.RegisterDefaults();

		foreach (FileInfo godotProject in godotProjects) {
			string projectDir = godotProject.DirectoryName ?? throw new ArgumentException("Invalid project.godot file path.");
			string projectFileContents = File.ReadAllText(godotProject.FullName);

			Match? assemblyMatch = GodotProjectAssemblyRegex().Match(projectFileContents);
			string? assemblyName = assemblyMatch?.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(assemblyName)) {
				Console.WriteLine($"Failed to find assembly name in {godotProject}");
				continue;
			}

			Match? nameMatch = GodotProjectNameRegex().Match(projectFileContents);
			string? projectName = nameMatch?.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(projectName)) {
				Console.WriteLine($"Failed to find project name in {godotProject}");
				continue;
			}


			// 1. Compile C# assemblies
			string? csproj = Directory.GetFiles(projectDir, $"{assemblyName}.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
			if (csproj is null) {
				Console.WriteLine($"No .csproj file found for {assemblyName} in {projectDir}");
				continue;
			}

			string? csprojContent = File.ReadAllText(csproj);
			// Get author name from the .csproj file
			if (author is null) {
				Match? authorMatch = CsprojAuthorRegex().Match(csprojContent);
				if (authorMatch?.Groups.Count > 1) {
					author = authorMatch.Groups[1].Value.Trim().Split([',', ';'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim()!;
				}
				else {
					Console.WriteLine($"Failed to find author in {csproj}");
					continue;
				}
			}

			if (version is null) {
				version = "1.0.0"; // Default version if not provided

				Match? versionMatch = CsprojVersionRegex().Match(csprojContent);
				if (versionMatch?.Groups.Count > 1) {
					version = versionMatch.Groups[1].Value.Trim();
				}
			}


			Console.WriteLine($"Building {assemblyName}.csproj ...");
			string? dllFile = BuildGodotCsproj(csproj);
			if (dllFile is null) {
				Console.WriteLine($"No DLL found for {assemblyName} in {projectDir}");
				continue;
			}
			string dllFileName = Path.GetFileName(dllFile);

			string assembliesPath = Path.Combine(output.FullName, "Assemblies");
			Directory.CreateDirectory(assembliesPath);

			string destPath = Path.Combine(assembliesPath, dllFileName);
			File.Copy(dllFile, destPath, overwrite: true);

			Console.WriteLine($"Successfully built {csproj} to {assembliesPath}");


			// 2. Pack assets into .pck file
			string assetsPath = Path.Combine(output.FullName, "Assets");
			Directory.CreateDirectory(assetsPath);

			string? pckName = $"{projectName}.pck";
			string? pckPath = Path.Combine(assetsPath, pckName);

			int packResult = RunProcess(godotExe?.FullName ?? "godot", $"--path \"{projectDir}\" --export-pack \"Windows Desktop\" \"{pckPath}\"");
			if (packResult != 0) {
				Console.WriteLine($"Failed to pack assets for {projectFileContents}");
				continue;
			}

			Console.WriteLine($"Successfully packed assets into {pckPath}");


			// 3. Create mod manifest
			string pascalCaseProjectName = ToPascalCase(projectName);
			string modManifestPath = Path.Combine(output.FullName, $"{pascalCaseProjectName}.mod.yaml");
			ModMetaData modMetaData = new() {
				Name = projectName,
				Version = version,
				Author = author,
				AssemblyPaths = [new FilePath($"Assemblies/{dllFileName}")],
				AssetPaths = [new FilePath($"Assets/{pckName}")],
			};

			File.WriteAllText(modManifestPath, ModMetaData.ToYaml(modMetaData));
		}
	}

	private static string ToPascalCase(string projectName) {
		if (string.IsNullOrWhiteSpace(projectName)) {
			throw new ArgumentException("Project name cannot be null or empty.", nameof(projectName));
		}
		string[] words = projectName.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries);
		if (words.Length == 0) return string.Empty;

		string pascalCase = string.Concat(words.Select(w => char.ToUpper(w[0]) + w[1..]));
		return pascalCase;
	}

	private static string? BuildGodotCsproj(string path) {
		string projectDir = Path.GetDirectoryName(path) ?? throw new ArgumentException("Invalid project file path.");
		string assemblyName = Path.GetFileNameWithoutExtension(path);

		int buildResult = RunProcess("dotnet", $"build \"{path}\" -c Release");
		if (buildResult != 0) {
			Console.WriteLine($"Failed to build {assemblyName}");
			return null;
		}
		string tempBinDir = Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "Release", "publish");
		if (!Directory.Exists(tempBinDir)) {
			Console.WriteLine($"Build output directory not found: {tempBinDir}");
			return null;
		}

		// Find all DLLs matching the assembly name
		return Directory.GetFiles(tempBinDir, $"{assemblyName}.dll", SearchOption.AllDirectories).FirstOrDefault();
	}

	private static Process? StartProcess(string fileName, string arguments) {
		ProcessStartInfo psi = new(fileName, arguments) {
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false
		};

		return Process.Start(psi);
	}
	private static int RunProcess(string fileName, string arguments) {
		using Process? proc = StartProcess(fileName, arguments);

		if (proc != null) {
			proc.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
			proc.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();
			proc.WaitForExit();

			return proc.ExitCode;
		}

		return -1;
	}
}