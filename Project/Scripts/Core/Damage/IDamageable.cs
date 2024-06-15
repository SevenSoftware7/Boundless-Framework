namespace LandlessSkies.Core;

using Godot;

public interface IDamageable {
	void Damage(float amount);
	void Kill();
}