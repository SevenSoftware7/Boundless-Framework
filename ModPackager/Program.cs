using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Build.Locator;
using Seven.Boundless.Modding;
using Seven.Boundless.Utility;

internal partial class Program {
	[GeneratedRegex(@"^project/assembly_name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectAssemblyRegex();

	[GeneratedRegex(@"^config/name\s*=\s*""?(.*?)""?$", RegexOptions.Multiline)]
	private static partial Regex GodotProjectNameRegex();


	private static async Task<int> Main(string[] args) {
		Option<FileInfo[]> projectsOption = new(["--projects", "-P"], "List of Godot project.godot files");
		Option<FileInfo?> godotExeOption = new(["--godot", "-g"], getDefaultValue: static () => null, description: "Path to the Godot executable");
		Option<DirectoryInfo> outputOption = new(["--output", "-o"], getDefaultValue: static () => new("./mod/"), description: "Output folder for .dll and .pck files");
		Option<string?> authorOption = new(["--author", "-a"], getDefaultValue: static () => null, description: "Author name to include in the mod manifest");
		Option<string?> versionOption = new(["--project-version", "-v"], getDefaultValue: static () => null, description: "Version to include in the mod manifest");
		Option<string?> descriptionOption = new(["--description", "-d"], getDefaultValue: static () => null, description: "Description to include in the mod manifest");
		Option<bool> packOption = new(["--pack", "-p"], getDefaultValue: static () => false, description: "Whether to pack the Mod into a .zip file");

		RootCommand? rootCommand = new("Godot Mod Packager") {
			projectsOption,
			godotExeOption,
			outputOption,
			authorOption,
			versionOption,
			descriptionOption,
			packOption
		};

		rootCommand.SetHandler(Execute, projectsOption, godotExeOption, outputOption, authorOption, versionOption, descriptionOption, packOption);

		return await rootCommand.InvokeAsync(args);
	}

	private static void Execute(FileInfo[] godotProjects, FileInfo? godotExe, DirectoryInfo outputPath, string? author, string? version, string? description, bool pack) {
		MSBuildLocator.RegisterDefaults();

		foreach (FileInfo godotProject in godotProjects) {
			DirectoryInfo projectDir = new(godotProject.DirectoryName ?? throw new ArgumentException("Invalid project.godot file path."));
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

			Match? nameMatch = GodotProjectNameRegex().Match(projectFileContents);
			string? projectName = nameMatch?.Groups[1].Value;
			if (string.IsNullOrWhiteSpace(projectName)) {
				Console.WriteLine("Failed to find project name in project.godot");
				continue;
			}

			DirectoryInfo filesOutputDir = new(Path.Combine(outputPath.FullName, projectName));
			if (!filesOutputDir.Exists) {
				filesOutputDir.Create();
			}

			// Step 1: Extract csproj info and build
			if (!TryProcessCsproj(projectFileContents, projectDir, ref author, ref version, ref description, out var csprojInfo))
				continue;


			// Step 2: Build the csproj and copy DLLs
			TryBuildAndCopyAssemblies(csprojInfo.CsprojPath, csprojInfo.AssemblyName, filesOutputDir, out var buildOutput);

			// Step 3: Pack assets into .pck file
			TryPackAssets(godotExe, projectDir, projectName, filesOutputDir, out FilePath? pckFilePath);

			// Step 4: Create mod manifest
			CreateModManifest(projectName, version, author, description, buildOutput?.ProjectDllPath.FullFileName, pckFilePath?.FullFileName, filesOutputDir);

			// Step 5: Optionally pack everything into a .zip file
			if (pack) {
				string zipFileName = $"{ToPascalCase(projectName)}.zip";
				string zipFilePath = Path.Combine(outputPath.FullName, zipFileName);
				if (File.Exists(zipFilePath)) {
					File.Delete(zipFilePath);
				}

				string[] files = Directory.GetFiles(filesOutputDir.FullName, "*", SearchOption.AllDirectories);

				using (ZipArchive zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create)) {
					foreach (string filePath in files) {
						string relativePath = Path.GetRelativePath(filesOutputDir.FullName, filePath);
						zipArchive.CreateEntryFromFile(filePath, relativePath, CompressionLevel.Optimal);
					}
				}

				filesOutputDir.Delete(true);

				Console.WriteLine($"Successfully created mod package: {zipFilePath}");
			}
		}
	}

	private static bool TryProcessCsproj(
		string projectFileContents,
		DirectoryInfo projectDir,
		[NotNullWhen(true)] ref string? author,
		[NotNullWhen(true)] ref string? version,
		[NotNullWhen(true)] ref string? description,
		out (string AssemblyName, FileInfo CsprojPath) result
	){
		result = default;

		Match? assemblyMatch = GodotProjectAssemblyRegex().Match(projectFileContents);
		string? assemblyName = assemblyMatch?.Groups[1].Value;
		if (string.IsNullOrWhiteSpace(assemblyName)) {
			Console.WriteLine("Failed to find assembly name in project.godot");
			return false;
		}

		FileInfo? csprojFilePath = projectDir.GetFiles($"{assemblyName}.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
		if (csprojFilePath is null) {
			Console.WriteLine($"No .csproj file found for {assemblyName} in {projectDir}");
			return false;
		}

		string csprojContent;
		using (StreamReader reader = csprojFilePath.OpenText()) {
			csprojContent = reader.ReadToEnd();
		}

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

		result = (assemblyName, csprojFilePath);
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
		FileInfo csprojPath,
		string assemblyName,
		DirectoryInfo output,
		[NotNullWhen(true)] out (FilePath ProjectDllPath, FilePath[] AccessoryDllPaths)? buildOutput
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
		DirectoryInfo projectDir,
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
		string? dllFileName,
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
			AssemblyPaths = dllFileName is not null ? [new FilePath($"Assemblies/{dllFileName}")] : [],
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

	private static (FilePath projectDll, FilePath[] accessoryDlls)? BuildGodotCsproj(FileInfo path) {
		string projectDir = path.DirectoryName ?? throw new ArgumentException("Invalid project file path.");
		string assemblyName = Path.GetFileNameWithoutExtension(path.Name);

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