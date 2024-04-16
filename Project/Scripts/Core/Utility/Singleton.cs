// namespace SevenGame.Utility;

// using Godot;
// using LandlessSkies.Core;

// public interface ISingleton<T> where T : Node, new() {
// 	private static T? Instance { get; set; } = null!;


// 	public static T? GetInstance() =>
// 		Instance;

// 	public static T GetOrCreateInstance(Node parent) =>
// 		Instance ??= new T().SetOwnerAndParent(parent);

// 	public static void SetInstance(T? newInstance) {
// 		if (Instance is not null && Instance != newInstance) {
// 			Instance?.QueueFree();
// 		}
// 		Instance = newInstance;
// 	}
// }