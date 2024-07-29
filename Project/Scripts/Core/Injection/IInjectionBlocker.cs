namespace LandlessSkies.Core;

using Godot;

public interface IInjectionBlocker<T> {
	bool ShouldBlock(Node parent, T value) => true;
}