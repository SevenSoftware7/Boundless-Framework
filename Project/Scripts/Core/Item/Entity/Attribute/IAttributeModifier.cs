namespace LandlessSkies.Core;

public interface IAttributeModifier {
	bool IsStacking { get; }
	float ApplyTo(float baseValue);
}