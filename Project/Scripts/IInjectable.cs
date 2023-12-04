

namespace LandlessSkies.Core;

public interface IInjectable<T> {
    public void Inject(T value) {}
}

public static class InjectableExtensions {
    /// <summary>
    /// Allows implicit use of the Interface with default implementation
    /// </summary>
    public static void Inject<T>(this IInjectable<T> injectable, T val) {
        injectable.Inject(val);
    }
}