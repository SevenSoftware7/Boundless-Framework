using System;
using SevenDev.Boundless.Utility;

namespace SevenDev.Boundless.Modding;

public struct ModInfo : IEquatable<ModInfo> {
	public FilePath ZipPath;
	public FilePath ManifestPath;
	public ModManifest Manifest;

	public ModInfo(FilePath zipPath, FilePath manifestPath, ModManifest manifest) {
		ZipPath = zipPath;
		ManifestPath = manifestPath;
		Manifest = manifest;
	}

	public readonly bool Equals(ModInfo other) => Manifest.Equals(other.Manifest);
	public static bool operator ==(ModInfo left, ModInfo right) => left.Equals(right);
	public static bool operator !=(ModInfo left, ModInfo right) => !(left == right);
	public override readonly bool Equals(object? obj) => obj is ModInfo info && Equals(info);

	public override readonly int GetHashCode() => Manifest.GetHashCode();
}