namespace LandlessSkies.Core;

using System;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class TraitResource : Resource, IEquatable<TraitResource> {
	private static readonly Trait defaultTrait = Traits.GenericTraits[0];

	public Trait Trait {
		get => _trait;
		set {
			_trait = value;
			ResourceName = _trait.Name;
			EmitChanged();
			NotifyPropertyListChanged();
		}
	}
	private Trait _trait = defaultTrait;

	[Export] private bool Dropdown {
		get => _useTraitDropdown;
		set {
			_useTraitDropdown = value;
			NotifyPropertyListChanged();
		}
	}
	private bool _useTraitDropdown = false;

	[Export]
	public string Name {
		get => Trait.Name;
		private set => Trait = value;
	}


	public TraitResource() : base() { }
	public TraitResource(Trait trait) : this() {
		Trait = trait;
	}


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (Dropdown && name == PropertyName.Name) {
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = Traits.JoinedGenericTraits;
		}
		else if (name == PropertyName.Dropdown) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}

	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property) || property == PropertyName.Name || property == PropertyName.Dropdown;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.Name) return defaultTrait.Name;
		if (property == PropertyName.Dropdown) return Dropdown;
		return base._PropertyGetRevert(property);
	}

	public bool Equals(TraitResource? other) {
		if (other is null) return false;
		if (ReferenceEquals(this, other)) return true;
		return Trait == other.Trait;
	}
	public override bool Equals(object? obj) {
		return Equals(obj as TraitResource);
	}


	public static bool operator ==(TraitResource? left, TraitResource? right) {
		if (left is null) return right is null;
		return left.Equals(right);
	}
	public static bool operator !=(TraitResource? left, TraitResource? right) {
		return !(left == right);
	}

	public static implicit operator Trait(TraitResource resource) => resource.Trait;
	public static implicit operator TraitResource(Trait trait) => new(trait);

	public override int GetHashCode() => Trait.GetHashCode();
}