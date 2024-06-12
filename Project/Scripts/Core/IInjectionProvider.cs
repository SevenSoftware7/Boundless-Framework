namespace LandlessSkies.Core;

public interface IInjectionProvider<T> : IInjectionBlocker<T> {
	T GetInjection();
}