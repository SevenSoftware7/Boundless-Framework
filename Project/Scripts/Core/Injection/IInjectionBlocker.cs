using Godot;

namespace LandlessSkies.Core;

public interface IInjectionBlocker<T> {
	bool ShouldBlock(Node parent, T value) => true;
}