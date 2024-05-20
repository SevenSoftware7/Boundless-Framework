namespace LandlessSkies.Core;

public interface ICustomizable : IUIObject {
	ICustomizable[] Customizables { get; }
	ICustomization[] Customizations { get; }
}