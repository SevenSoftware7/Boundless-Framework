namespace SevenDev.Utility;

using Godot;

public static class ResourceExtensions {
	public static StringName GetFileName(this Resource resource) {
		return resource.ResourcePath.Split('/')[^1].Replace(".tres", "");
	}
}