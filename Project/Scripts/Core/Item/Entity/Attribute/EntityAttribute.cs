namespace LandlessSkies.Core;

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Godot;

public readonly struct EntityAttribute {
	public readonly StringName Name { get; init; }

	public EntityAttribute(string name) {
		Name = name.ToSnakeCase();
	}


	public static implicit operator EntityAttribute(StringName name) => new(name);
	public static implicit operator EntityAttribute(string name) => new(name);

	public override bool Equals([NotNullWhen(true)] object? obj) => obj switch {
		string otherString => Equals(otherString),
		StringName otherStringName => Equals(otherStringName),
		EntityAttribute otherAttribute => Equals(otherAttribute),
		_ => false
	};

	public static bool operator ==(EntityAttribute? left, EntityAttribute? right) {
		return left?.Equals(right) ?? right is null;
	}
	public static bool operator !=(EntityAttribute? left, EntityAttribute? right) {
		return !(left == right);
	}

	public bool Equals(EntityAttribute? other) => Name == other?.Name;


	public static bool operator ==(EntityAttribute? left, StringName? right) {
		return left?.Equals(right) ?? right is null;
	}
	public static bool operator !=(EntityAttribute? left, StringName? right) {
		return !(left == right);
	}

	public static bool operator ==(StringName? left, EntityAttribute? right) {
		return right?.Equals(left) ?? left is null;
	}
	public static bool operator !=(StringName? left, EntityAttribute? right) {
		return !(left == right);
	}

	public bool Equals(StringName? other) => Name == other;


	public static bool operator ==(EntityAttribute? left, string? right) {
		return left?.Equals(right) ?? right == null;
	}
	public static bool operator !=(EntityAttribute? left, string? right) {
		return !(left == right);
	}

	public static bool operator ==(string? left, EntityAttribute? right) {
		return right?.Equals(left) ?? left == null;
	}
	public static bool operator !=(string? left, EntityAttribute? right) {
		return !(left == right);
	}

	public bool Equals(string? other) => other is not null && Name == other;


	public override int GetHashCode() => Name.GetHashCode();
	public override string ToString() => Name;
}