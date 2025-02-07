namespace LandlessSkies.Core;

public interface ITraitModifier {
	bool IsStacking { get; }
	float ApplyTo(float baseValue);
}