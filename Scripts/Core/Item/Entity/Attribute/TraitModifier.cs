namespace LandlessSkies.Core;

using System;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public abstract partial class TraitModifier : Resource, IEquatable<TraitModifier>, ITraitModifier {
	[Export] private bool UseTraitDropdown {
		get => _useTraitDropdown;
		set {
			_useTraitDropdown = value;
			NotifyPropertyListChanged();
		}
	}
	private bool _useTraitDropdown = false;

	[Export]
	public StringName TraitName {
		get => Target.Name;
		private set {
			Target = value;
			UpdateName();
			EmitChanged();
		}
	}

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float Efficiency {
		get => _efficiency;
		set {
			_efficiency = Mathf.Clamp(value, 0f, 1f);
			EmitValueModified();
		}
	}
	private float _efficiency = 1f;

	public Trait Target { get; private set; } = Traits.GenericTraits[0];
	public virtual bool IsStacking => false;

	public event Action<Trait>? OnValueModified;
	protected void EmitValueModified() {
		OnValueModified?.Invoke(Target);
		UpdateName();
	}



	public TraitModifier(Trait target) : base() {
		Target = target;
	}
	protected TraitModifier() : base() { }



	public abstract float ApplyTo(float baseValue);

	private void UpdateName() {
		if (!Engine.IsEditorHint()) return;

		ResourceName = GetResourceName();
	}
	protected abstract string GetResourceName();


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (UseTraitDropdown && name == PropertyName.TraitName) {
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = Traits.JoinedGenericTraits;
		}
		else if (name == PropertyName.UseTraitDropdown) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}

	public bool Equals(TraitModifier? other) {
		if (other is null) return false;
		if (!EqualsInternal(other)) return true;
		return Target == other.Target;
	}
	protected abstract bool EqualsInternal(TraitModifier other);
	public override bool Equals(object? obj) => Equals(obj as TraitModifier);


	public static bool operator ==(TraitModifier? left, TraitModifier? right) {
		if (left is null) return right is null;
		return left.Equals(right);
	}
	public static bool operator !=(TraitModifier? left, TraitModifier? right) {
		return !(left == right);
	}

	public override int GetHashCode() => (Target.GetHashCode() * 397) ^ (Efficiency.GetHashCode() * 397);
}