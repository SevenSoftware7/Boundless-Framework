using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Build.Locator;
using SevenDev.Boundless.Modding;
using SevenDev.Boundless.Utility;

internal partial class Program {
	[GeneratedRegex(@"^project/assembly_name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectAssemblyRegex();

	[GeneratedRegex(@"^config/name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectNameRegex();


	private static async Task<int> Main(string[] args) {
		Option<FileInfo[]> projectsOption = new(["--projects", "-p"], "List of Godot project.godot files");
		Option<FileInfo?> godotExeOption = new(["--godot", "-g"], getDefaultValue: static () => null, description: "Path to the Godot executable");
		Option<DirectoryInfo> outputOption = new(["--output", "-o"], getDefaultValue: static () => new("./mod/"), description: "Output folder for .dll and .pck files");
		Option<string?> authorOption = new(["--author", "-a"], getDefaultValue: static () => null, description: "Author name to include in the mod manifest");
		Option<string?> versionOption = new(["--project-version", "-v"], getDefaultValue: static () => null, description: "Version to include in the mod manifest");
		Option<string?> descriptionOption = new(["--description", "-d"], getDefaultValue: static () => null, description: "Description to include in the mod manifest");

		RootCommand? rootCommand = new("Godot Mod Packager") {
			projectsOption,
			godotExeOption,
			outputOption,
			authorOption,
			versionOption,
			descriptionOption
		};

		rootCommand.SetHandler(Execute, projectsOption, godotExeOption, outputOption, authorOption, versionOption, descriptionOption);

		return await rootCommand.InvokeAsync(args);
	}

	private static void Execute(FileInfo[] godotProjects, FileInfo? godotExe, DirectoryInfo outputPath, string? author, string? version, string? description) {
		MSBuildLocator.RegisterDefaults();

		foreach (FileInfo godotProject in godotProjects) {
			string projectDir = godotProject.DirectoryName ?? throw new ArgumentException("Invalid project.godot file path.");
			string projectName = Path.GetFileNameWithoutExtension(godotProject.Name);
			string projectFileContents = File.ReadAllText(godotProject.FullName);

			if (godotExe is null) {
				// Try to find Godot executable in PATH
				godotExe = new FileInfo("godot");
				if (!godotExe.Exists) {
					Console.WriteLine("Godot executable not found. Please specify the path using --godot option.");
					return;
				}
			}
			else if (!godotExe.Exists) {
				Console.WriteLine($"Godot executable not found at {godotExe.FullName}");
				return;
			}

			// Step 1: Extract csproj info and build
			if (!TryProcessCsproj(projectFileContents, projectDir, ref author, ref version, ref description, out var csprojInfo))
				continue;

			// Step 2: Build the csproj and copy DLLs
			if (!TryBuildAndCopyAssemblies(csprojInfo.CsprojPath, csprojInfo.AssemblyName, outputPath, out var buildOutput))
				continue;


			// Step 3: Pack assets into .pck file
			TryPackAssets(godotExe, projectDir, projectName, outputPath, out FilePath? pckFilePath);

			// Step 4: Create mod manifest
			CreateModManifest(projectName, version, author, description, buildOutput.ProjectDllPath.FullFileName, pckFilePath?.FullFileName, outputPath);
		}
	}

	private static bool TryProcessCsproj(
		string projectFileContents,
		string projectDir,
		[NotNullWhen(true)] ref string? author,
		[NotNullWhen(true)] ref string? version,
		[NotNullWhen(true)] ref string? description,
		out (string AssemblyName, string ProjectName, string CsprojPath) result
	){
		result = default;

		Match? assemblyMatch = GodotProjectAssemblyRegex().Match(projectFileContents);
		string? assemblyName = assemblyMatch?.Groups[1].Value;
		if (string.IsNullOrWhiteSpace(assemblyName)) {
			Console.WriteLine("Failed to find assembly name in project.godot");
			return false;
		}

		Match? nameMatch = GodotProjectNameRegex().Match(projectFileContents);
		string? projectName = nameMatch?.Groups[1].Value;
		if (string.IsNullOrWhiteSpace(projectName)) {
			Console.WriteLine("Failed to find project name in project.godot");
			return false;
		}

		string? csprojFilePath = Directory.GetFiles(projectDir, $"{assemblyName}.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
		if (csprojFilePath is null) {
			Console.WriteLine($"No .csproj file found for {assemblyName} in {projectDir}");
			return false;
		}

		string csprojContent = File.ReadAllText(csprojFilePath);

		if (string.IsNullOrWhiteSpace(author)) {
			author = ExtractCsprojField(csprojContent, "Authors?", "author", true);
			if (string.IsNullOrWhiteSpace(author)) {
				Console.WriteLine("Failed to find author in .csproj");
				return false;
			}
		}

		if (string.IsNullOrWhiteSpace(version)) {
			version = ExtractCsprojField(csprojContent, "Version", "version");
			if (string.IsNullOrWhiteSpace(version)) {
				Console.WriteLine("Failed to find version in .csproj");
				return false;
			}
		}

		if (string.IsNullOrWhiteSpace(description)) {
			description = ExtractCsprojField(csprojContent, "Description", "description");
			description ??= "";
		}

		result = (assemblyName, projectName, csprojFilePath);
		return true;
	}

	private static string? ExtractCsprojField(
		string csprojContent,
		[StringSyntax(StringSyntaxAttribute.Regex, nameof(fieldnameRegex))] string fieldnameRegex,
		string fieldName,
		bool split = false
	) {
		Match? match = Regex.Match(csprojContent, @$"<{fieldnameRegex}>(.*?)<\/{fieldnameRegex}>", RegexOptions.Multiline);
		if (match?.Groups.Count > 1) {
			string value = match.Groups[1].Value.Trim();
			if (split) value = value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "";

			if (!string.IsNullOrWhiteSpace(value)) return value;
		}

		Console.WriteLine($"Failed to find {fieldName} in .csproj");
		return null;
	}

	private static bool TryBuildAndCopyAssemblies(
		string csprojPath,
		string assemblyName,
		DirectoryInfo output,
		[NotNullWhen(true)] out (FilePath ProjectDllPath, FilePath[] AccessoryDllPaths) buildOutput
	) {
		buildOutput = default;

		Console.WriteLine($"Building {assemblyName}.csproj ...");
		var buildResult = BuildGodotCsproj(csprojPath);
		if (buildResult is null) {
			Console.WriteLine($"No DLL found for {assemblyName}");
			return false;
		}

		string assembliesPath = Path.Combine(output.FullName, "Assemblies");
		Directory.CreateDirectory(assembliesPath);

		string destPath = Path.Combine(assembliesPath, buildResult.Value.projectDll.FullFileName);
		File.Copy(buildResult.Value.projectDll.Path, destPath, overwrite: true);
		foreach (FilePath accessoryDll in buildResult.Value.accessoryDlls) {
			string accessoryDestPath = Path.Combine(assembliesPath, accessoryDll.FullFileName);
			File.Copy(accessoryDll.Path, accessoryDestPath, overwrite: true);
		}

		Console.WriteLine($"Successfully built {csprojPath} to {assembliesPath}");
		buildOutput = (buildResult.Value.projectDll, buildResult.Value.accessoryDlls);
		return true;
	}

	private static bool TryPackAssets(
		FileInfo godotExe,
		string projectDir,
		string projectName,
		DirectoryInfo output,
		[NotNullWhen(true)] out FilePath? pckFilePath
	) {
		string assetsPath = Path.Combine(output.FullName, "Assets");
		Directory.CreateDirectory(assetsPath);

		string pckFileName = $"{projectName}.pck";
		pckFilePath = new(Path.Combine(assetsPath, pckFileName));

		int packResult = RunProcess(godotExe.FullName, $"--headless --path \"{projectDir}\" --export-pack \"Windows Desktop\" \"{pckFilePath.Value.Path}\"");
		if (packResult != 0) {
			Console.WriteLine($"Failed to pack assets for {projectName}");
			return false;
		}

		Console.WriteLine($"Successfully packed assets into {assetsPath}");
		return true;
	}

	private static ModManifest CreateModManifest(
		string projectName,
		string version,
		string author,
		string description,
		string dllFileName,
		string? pckFileName,
		DirectoryInfo outputPath
	) {
		string pascalCaseProjectName = ToPascalCase(projectName);
		string modManifestPath = Path.Combine(outputPath.FullName, $"{pascalCaseProjectName}.mod.yaml");

		ModManifest modManifest = new() {
			Name = projectName,
			Version = version,
			Author = author,
			Description = description,
			AssemblyPaths = [new FilePath($"Assemblies/{dllFileName}")],
			AssetPaths = pckFileName is not null ? [new FilePath($"Assets/{pckFileName}")] : [],
		};

		File.WriteAllText(modManifestPath, ModManifest.ToYaml(modManifest));

		return modManifest;
	}

	private static string ToPascalCase(string projectName) {
		if (string.IsNullOrWhiteSpace(projectName)) {
			throw new ArgumentException("Project name cannot be null or empty.", nameof(projectName));
		}
		string[] words = projectName.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries);
		if (words.Length == 0) return string.Empty;

		string pascalCase = string.Concat(words.Select(static w => char.ToUpper(w[0]) + w[1..]));
		return pascalCase;
	}

	private static (FilePath projectDll, FilePath[] accessoryDlls)? BuildGodotCsproj(string path) {
		string projectDir = Path.GetDirectoryName(path) ?? throw new ArgumentException("Invalid project file path.");
		string assemblyName = Path.GetFileNameWithoutExtension(path);

		int buildResult = RunProcess("dotnet", $"publish \"{path}\" -c Release");
		if (buildResult != 0) {
			Console.WriteLine($"Failed to build {assemblyName}");
			return null;
		}
		string tempBinDir = Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "Release", "publish");
		if (!Directory.Exists(tempBinDir)) {
			Console.WriteLine($"Build output directory not found: {tempBinDir}");
			return null;
		}

		FilePath[] dllFiles = [..
			Directory.GetFiles(tempBinDir, "*.dll", SearchOption.AllDirectories)
				.Select(filePath => new FilePath(filePath))
		];
		FilePath? projectDll = dllFiles.FirstOrDefault(file => file.FileName == assemblyName);
		if (!projectDll.HasValue) {
			Console.WriteLine($"No DLL found for {assemblyName} in {tempBinDir}");
			return null;
		}
		Console.WriteLine($"Found project DLL: {projectDll.Value}");

		dllFiles = [.. dllFiles.Where(file => Path.GetFileNameWithoutExtension(file) != assemblyName)];

		return (projectDll.Value, dllFiles);
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
			proc.OutputDataReceived += static (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
			proc.ErrorDataReceived += static (sender, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();
			proc.WaitForExit();

			return proc.ExitCode;
		}

		return -1;
	}
}