namespace LandlessSkies.Core;

using Godot;

public interface IInjectionInterceptor<T> {
	public T Intercept(Node child);
}