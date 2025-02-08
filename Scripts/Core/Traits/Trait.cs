namespace LandlessSkies.Core;

using System.Diagnostics.CodeAnalysis;
using Godot;

public readonly struct Trait {
	public readonly StringName Name { get; init; }

	public Trait(string name) {
		Name = name.ToSnakeCase();
	}


	public static implicit operator Trait(StringName name) => new(name);
	public static implicit operator Trait(string name) => new(name);

	public override bool Equals([NotNullWhen(true)] object? obj) => obj switch {
		string otherString => Equals(otherString),
		StringName otherStringName => Equals(otherStringName),
		Trait otherTrait => Equals(otherTrait),
		_ => false
	};


	public bool Equals(Trait? other) => Name == other?.Name;

	public static bool operator ==(Trait? left, Trait? right) {
		return left?.Equals(right) ?? right is null;
	}
	public static bool operator !=(Trait? left, Trait? right) {
		return !(left == right);
	}


	public bool Equals(StringName? other) => Name == other;

	public static bool operator ==(Trait? left, StringName? right) {
		return left?.Equals(right) ?? right is null;
	}
	public static bool operator !=(Trait? left, StringName? right) {
		return !(left == right);
	}

	public static bool operator ==(StringName? left, Trait? right) {
		return right?.Equals(left) ?? left is null;
	}
	public static bool operator !=(StringName? left, Trait? right) {
		return !(left == right);
	}


	public bool Equals(string? other) => other is not null && Name == other;

	public static bool operator ==(Trait? left, string? right) {
		return left?.Equals(right) ?? right == null;
	}
	public static bool operator !=(Trait? left, string? right) {
		return !(left == right);
	}

	public static bool operator ==(string? left, Trait? right) {
		return right?.Equals(left) ?? left == null;
	}
	public static bool operator !=(string? left, Trait? right) {
		return !(left == right);
	}


	public override int GetHashCode() => Name.GetHashCode();
	public override string ToString() => Name;
}