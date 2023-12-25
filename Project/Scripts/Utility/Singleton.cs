using Godot;

namespace SevenGame.Utility;

public interface ISingleton<T> where T : Node {
	public static T? Instance;
}


public static class SingletonHelper {
	public static T? GetInstance<T>() where T : Node {
		return ISingleton<T>.Instance;
	}

	public static void SetInstance<T>(T newInstance) where T : Node {
		if (ISingleton<T>.Instance is not null && ISingleton<T>.Instance != newInstance) {
			newInstance.QueueFree();
		}
		ISingleton<T>.Instance = newInstance;
	}
}