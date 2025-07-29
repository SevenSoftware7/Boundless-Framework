namespace Seven.Boundless;

public interface IDamageable {
	public void Damage(ref DamageData data);
	public void Kill();
}