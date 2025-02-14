namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public partial class TraitModifierApplier : Node {
	private readonly uint _durationMsec;
	private float progress = 0f;
	private readonly TraitModifier _modifier;
	private readonly TraitModifierCollection _modifierCollection;


	public TraitModifierApplier(uint durationMsec, TraitModifierCollection entity, TraitModifier modifier) : base() {
		_durationMsec = durationMsec;
		_modifierCollection = entity;
		_modifier = modifier;
	}

	public override void _Ready() {
		base._Ready();

		if (_modifierCollection.Contains(_modifier)) {
			QueueFree();
			return;
		}
		_modifierCollection.Add(_modifier);
	}

	public override void _Process(double delta) {
		base._Process(delta);
		try {
			_modifierCollection.SetMultiplier(_modifier, Mathf.Lerp(1f, 0f, progress / _durationMsec));
		}
		catch (KeyNotFoundException) {
			QueueFree();
			return;
		}

		if (progress >= _durationMsec) {
			_modifierCollection.Remove(_modifier);
			QueueFree();
			return;
		}

		progress = Mathf.Min(progress + 1000f * (float)delta, _durationMsec);
	}
}