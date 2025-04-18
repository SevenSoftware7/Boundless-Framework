namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public partial class TraitModifierRemover : TraitModifierOperation {
	public required uint DurationMsec { get; init; }
	public uint DelayMsec { get; init; } = 0;
	public TraitModifierCollection.InterpFunction InterpolationFunction { get; init; } = Mathf.Lerp;
	private double elapsed = 0f;


	public TraitModifierRemover(TraitModifierCollection collection, ITraitModifier modifier) : base(collection, modifier) { }

	public override void _Ready() {
		base._Ready();

		if (!ModifierCollection.Contains(Modifier)) {
			QueueFree();
			return;
		}

		if (DurationMsec == 0 && DelayMsec == 0) {
			GD.Print("Immediate");
			ModifierCollection.Remove(Modifier);
			QueueFree();
			return;
		}
		ModifierCollection.SetMultiplier(Modifier, 1f);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		try {
			double t = (elapsed - DelayMsec) / DurationMsec;
			t = Mathf.Clamp(t, 0, 1);
			ModifierCollection.SetMultiplier(Modifier, (float)InterpolationFunction(1, 0, t));
		}
		catch (KeyNotFoundException) {
			QueueFree();
			return;
		}

		if (elapsed >= DurationMsec + DelayMsec) {
			ModifierCollection.Remove(Modifier);
			QueueFree();
			return;
		}

		elapsed += 1000 * delta;
	}

	public override void _Notification(int what) {
		base._Notification(what);
		if (what != NotificationPredelete) return;


		ModifierCollection.Remove(Modifier);
	}

}