namespace SevenDev.Boundless;

using System;

public readonly struct StyleState : IEquatable<StyleState>, IComparable<StyleState> {
	public static readonly StyleState Primary = new(0);
	public static readonly StyleState Secondary = new(1);
	public static readonly StyleState Tertiary = new(2);
	public static readonly StyleState Quaternary = new(3);
	public static readonly StyleState Quinary = new(4);
	public static readonly StyleState Senary = new(5);
	public static readonly StyleState Septenary = new(6);
	public static readonly StyleState Octonary = new(7);
	public static readonly StyleState Nonary = new(8);
	public static readonly StyleState Denary = new(9);


	private readonly byte _value = 0;

	private StyleState(byte value) {
		_value = value;
	}


	public StyleState WrappedWith(StyleState max) => _value % (max._value + 1);
	public StyleState Wrap(StyleState toWrap) => toWrap.WrappedWith(this);



	public static implicit operator StyleState(uint style) => (byte)style;
	public static implicit operator StyleState(int style) => (byte)style;
	public static implicit operator StyleState(byte style) => style switch {
		0 => Primary,
		1 => Secondary,
		2 => Tertiary,
		3 => Quaternary,
		4 => Quinary,
		5 => Senary,
		6 => Septenary,
		7 => Octonary,
		8 => Nonary,
		9 => Denary,
		_ => new(style)
	};

	public static explicit operator byte(StyleState style) => style._value;
	public static explicit operator uint(StyleState style) => style._value;
	public static explicit operator int(StyleState style) => style._value;

	public int CompareTo(StyleState other) => _value.CompareTo(other._value);
	public static bool operator <(StyleState left, StyleState right) => left.CompareTo(right) < 0;
	public static bool operator <=(StyleState left, StyleState right) => left.CompareTo(right) <= 0;
	public static bool operator >(StyleState left, StyleState right) => left.CompareTo(right) > 0;
	public static bool operator >=(StyleState left, StyleState right) => left.CompareTo(right) >= 0;

	public bool Equals(StyleState other) => _value.CompareTo(other._value) == 0;
	public override bool Equals(object? obj) => obj is StyleState other && Equals(other);

	public static bool operator ==(StyleState left, StyleState right) => left.Equals(right);
	public static bool operator !=(StyleState left, StyleState right) => !(left == right);

	public static StyleState operator +(StyleState left, StyleState right) => left._value + right._value;
	public static StyleState operator -(StyleState left, StyleState right) => left._value - right._value;

	public static StyleState operator ++(StyleState style) => style._value + 1;
	public static StyleState operator --(StyleState style) => style._value - 1;


	public override int GetHashCode() => _value.GetHashCode();
	public override string ToString() => _value switch {
		0 => "Primary",
		1 => "Secondary",
		2 => "Tertiary",
		3 => "Quaternary",
		4 => "Quinary",
		5 => "Senary",
		6 => "Septenary",
		7 => "Octonary",
		8 => "Nonary",
		9 => "Denary",
		_ => _value.ToString()
	};
}