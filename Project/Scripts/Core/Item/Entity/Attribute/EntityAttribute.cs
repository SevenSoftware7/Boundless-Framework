namespace LandlessSkies.Core;

using System.Diagnostics.CodeAnalysis;
using Godot;

public readonly struct EntityAttribute {
	public readonly StringName Name { get; init; }

	public EntityAttribute(string name) {
		Name = name.ToSnakeCase();
	}


	public static implicit operator EntityAttribute(StringName name) => new(name);
	public static implicit operator EntityAttribute(string name) => new(name);

	public override bool Equals([NotNullWhen(true)] object? obj) {
		StringName? otherName = obj switch {
			string otherString => otherString,
			StringName otherStringName => otherStringName,
			EntityAttribute otherAttribute => otherAttribute.Name,
			_ => null
		};
		return otherName is not null && Name == otherName;
	}

	public static bool operator ==(EntityAttribute left, EntityAttribute right) {
		return left.Equals(right);
	}

	public static bool operator !=(EntityAttribute left, EntityAttribute right) {
		return !(left == right);
	}

	public override int GetHashCode() => Name.GetHashCode();
	public override string ToString() => Name;
}