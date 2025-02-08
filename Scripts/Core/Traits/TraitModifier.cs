namespace LandlessSkies.Core;

using System;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class TraitModifier : Resource, IEquatable<TraitModifier>, ITraitModifier {
	public Trait Trait {
		get => _traitResource.Trait;
		private set {
			_traitResource.Trait = value;
			EmitValueModified();
		}
	}
	[Export] protected TraitResource TraitResource {
		get => _traitResource;
		private set => Trait = value.Trait;
	}
	private readonly TraitResource _traitResource = new();


	[Export(PropertyHint.Range, "0,1,0.01")]
	public float Efficiency {
		get => _efficiency;
		set {
			_efficiency = Mathf.Clamp(value, 0f, 1f);
			EmitValueModified();
		}
	}
	private float _efficiency = 1f;

	public virtual bool IsStacking => false;

	public event Action<Trait>? OnValueModified;
	protected void EmitValueModified() {
		OnValueModified?.Invoke(Trait);
		UpdateName();
	}



	public TraitModifier(Trait target) : this() {
		Trait = target;
	}
	protected TraitModifier() : base() {
		_traitResource.Changed += EmitValueModified;
	}



	public abstract float ApplyTo(float baseValue);

	private void UpdateName() {
		if (!Engine.IsEditorHint()) return;

		ResourceName = GetResourceName();
	}
	protected abstract string GetResourceName();


	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property) || property == PropertyName.TraitResource;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.TraitResource) return _traitResource;
		return base._PropertyGetRevert(property);
	}


	protected abstract bool EqualsInternal(TraitModifier other);
	public bool Equals(TraitModifier? other) {
		if (other is null) return false;
		if (!EqualsInternal(other)) return false;
		return Trait == other.Trait;
	}
	public override bool Equals(object? obj) {
		return Equals(obj as TraitModifier);
	}


	public static bool operator ==(TraitModifier? left, TraitModifier? right) {
		if (left is null) return right is null;
		return left.Equals(right);
	}
	public static bool operator !=(TraitModifier? left, TraitModifier? right) {
		return !(left == right);
	}

	public override int GetHashCode() => (Trait.GetHashCode() * 397) ^ (Efficiency.GetHashCode() * 397);
}