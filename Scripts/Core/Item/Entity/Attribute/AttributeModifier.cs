namespace LandlessSkies.Core;

using System;
using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public abstract partial class AttributeModifier : Resource, IEquatable<AttributeModifier>, IAttributeModifier {
	public static readonly StringName AttributeValue = "AttributeValue";

	[Export] private bool UseAttributeDropdown {
		get => _useAttributeDropdown;
		set {
			_useAttributeDropdown = value;
			NotifyPropertyListChanged();
		}
	}
	private bool _useAttributeDropdown = false;

	[Export]
	public StringName AttributeName {
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

	public EntityAttribute Target { get; private set; } = Attributes.GenericAttributes[0];
	public virtual bool IsStacking => false;

	public event Action<EntityAttribute>? OnValueModified;
	protected void EmitValueModified() {
		OnValueModified?.Invoke(Target);
		UpdateName();
	}



	public AttributeModifier(EntityAttribute target) : base() {
		Target = target;
	}
	protected AttributeModifier() : base() { }



	public abstract float ApplyTo(float baseValue);

	private void UpdateName() {
		if (!Engine.IsEditorHint()) return;

		ResourceName = GetResourceName();
	}
	protected abstract string GetResourceName();


	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();

		if (UseAttributeDropdown && name == PropertyName.AttributeName) {
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = Attributes.JoinedGenericAttributes;
		}
		else if (name == PropertyName.UseAttributeDropdown) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
		}
	}

	public bool Equals(AttributeModifier? other) {
		if (other is null) return false;
		if (!EqualsInternal(other)) return true;
		return Target == other.Target;
	}
	protected abstract bool EqualsInternal(AttributeModifier other);
	public override bool Equals(object? obj) => Equals(obj as AttributeModifier);


	public static bool operator ==(AttributeModifier? left, AttributeModifier? right) {
		if (left is null) return right is null;
		return left.Equals(right);
	}
	public static bool operator !=(AttributeModifier? left, AttributeModifier? right) {
		return !(left == right);
	}

	public override int GetHashCode() => (Target.GetHashCode() * 397) ^ (Efficiency.GetHashCode() * 397);
}