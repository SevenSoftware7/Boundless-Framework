using Godot;

namespace SevenGame.Utility;

public interface ISingleton<T> where T : Node {
	public static T? Instance { get; private set; }


	public static void SetInstance(T newInstance) {
		if (Instance is not null && Instance != newInstance) {
			newInstance.QueueFree();
		}
		Instance = newInstance;
	}
}