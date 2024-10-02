namespace LandlessSkies.Core;

public interface IDamageable {
	void Damage(ref DamageData data);
	void Kill();
}