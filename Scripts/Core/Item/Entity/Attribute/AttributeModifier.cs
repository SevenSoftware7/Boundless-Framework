namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public abstract partial class AttributeModifier : Resource, IAttributeModifier {
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

	public EntityAttribute Target { get; private set; } = Attributes.GenericAttributes[0];
	public virtual bool IsStacking => false;

	public event System.Action<EntityAttribute>? OnValueModified;
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
}