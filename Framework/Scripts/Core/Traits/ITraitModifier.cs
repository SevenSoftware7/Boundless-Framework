namespace SevenDev.Boundless;

using System;

public interface ITraitModifier {
	public Trait Trait { get; }
	public event Action<Trait>? OnValueModified;

	public bool IsStacking { get; }
	public float ApplyTo(float baseValue, float multiplier = 1f);
}