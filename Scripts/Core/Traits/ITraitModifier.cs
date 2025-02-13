namespace LandlessSkies.Core;

public interface ITraitModifier {
	public bool IsStacking { get; }
	public float ApplyTo(float baseValue, float multiplier = 1f);
}