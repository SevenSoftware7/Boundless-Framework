namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public abstract partial class AttributeModifier : Resource, IAttributeModifier {
	public static readonly StringName AttributeValue = "AttributeValue";

	[Export]
	public StringName Name {
		get => Target.Name;
		set {
			Target = value;
			EmitChanged();
		}
	}
	public EntityAttribute Target { get; private set; } = Attributes.GenericAttributes[0];
	public virtual bool IsStacking => false;



	public AttributeModifier(EntityAttribute target) : base() {
		Target = target;
	}
	protected AttributeModifier() : base() { }



	public abstract float ApplyTo(float baseValue);
	public void UpdateName() {
		if (!Engine.IsEditorHint()) return;

		ResourceName = GetResourceName();
	}
	protected abstract string GetResourceName();

	public override Array<Dictionary> _GetPropertyList() {
		return [new Dictionary() {
			{ "name", AttributeValue },
			{ "type", (int)Variant.Type.StringName },
			{ "usage", (int)(PropertyUsageFlags.Default & ~PropertyUsageFlags.Storage) },
			{ "hint", (int)PropertyHint.Enum },
			{ "hint_string", string.Join(',', Attributes.GenericAttributes) },
		}];
	}

	public override Variant _Get(StringName property) {
		if (property != AttributeValue)
			return base._Get(property);

		return Name;
	}

	public override bool _Set(StringName property, Variant value) {
		if (property != AttributeValue)
			return base._Set(property, value);

		Name = value.AsStringName();
		UpdateName();
		return true;
	}
}