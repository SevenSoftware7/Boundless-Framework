using Godot;

namespace LandlessSkies.Core;

public interface ICustomizable {
	IUIObject UIObject { get; }
	ICustomizable[] Children { get; }
	ICustomizationParameter[] Customizations { get; }
}