namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public partial class TraitModifierAdder : TraitModifierOperation {
	public required uint DurationMsec { get; init; }
	public uint DelayMsec { get; init; } = 0;
	public TraitModifierCollection.InterpFunction InterpolationFunction { get; init; } = Mathf.Lerp;
	private double elapsed = 0f;


	public TraitModifierAdder(TraitModifierCollection collection, ITraitModifier modifier) : base(collection, modifier) { }

	public override void _Ready() {
		base._Ready();

		if (ModifierCollection.Contains(Modifier)) {
			QueueFree();
			return;
		}
		ModifierCollection.Add(Modifier);

		if (DurationMsec == 0 && DelayMsec == 0) {
			QueueFree();
			return;
		}
		ModifierCollection.SetMultiplier(Modifier, 0f);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		try {
			double t = (elapsed - DelayMsec) / DurationMsec;
			t = Mathf.Clamp(t, 0, 1);
			ModifierCollection.SetMultiplier(Modifier, (float)InterpolationFunction(0, 1, t));
		}
		catch (KeyNotFoundException) {
			QueueFree();
			return;
		}

		if (elapsed >= DurationMsec + DelayMsec) {
			QueueFree();
			return;
		}

		elapsed += 1000 * delta;
	}
}