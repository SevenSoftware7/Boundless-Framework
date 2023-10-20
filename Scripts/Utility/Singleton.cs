using Godot;


namespace SevenGame.Utility;

public interface ISingleton<T> where T : Node {

    public static T instance;
    
}

public static class SingletonHelper {
    public static T GetInstance<T>() where T : Node {
        return ISingleton<T>.instance;
    }

    public static void SetInstance<T>(T newInstance) where T : Node {
        if (ISingleton<T>.instance is not null && ISingleton<T>.instance != newInstance) {
            newInstance.QueueFree();
        }
        ISingleton<T>.instance = newInstance;
    }
}
