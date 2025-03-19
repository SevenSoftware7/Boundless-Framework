namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class TraitResource : Resource, IEquatable<TraitResource> {
	private static Trait DefaultTrait => Traits.GenericTraits[0];
	private readonly string DropdownTraitsString = Traits.JoinedGenericTraits;

	public Trait Trait {
		get;
		set {
			field = value;
			ResourceName = field.Name;
		}
	} = DefaultTrait;

	[Export] private bool Dropdown {
		get;
		set {
			field = value;
			NotifyPropertyListChanged();
		}
	} = false;

	[Export]
	public string Name {
		get => Trait.Name;
		private set => Trait = value;
	}


	public TraitResource() : base() { }
	public TraitResource(Trait trait, IEnumerable<Trait> dropdownTraits = null!) : this() {
		Trait = trait;
		if (dropdownTraits is not null) {
			DropdownTraitsString = string.Join(',', dropdownTraits);
		}
	}


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (Dropdown && name == PropertyName.Name) {
			property["hint"] = (long)PropertyHint.Enum;
			property["hint_string"] = DropdownTraitsString;
		}
		else if (name == PropertyName.Dropdown) {
			property["usage"] = (long)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}

	public override bool _PropertyCanRevert(StringName property) {
		return base._PropertyCanRevert(property)
			|| property == PropertyName.Name
			|| property == PropertyName.Dropdown;
	}
	public override Variant _PropertyGetRevert(StringName property) {
		if (property == PropertyName.Name) return DefaultTrait.Name;
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

	public override int GetHashCode() => Trait.GetHashCode();
}