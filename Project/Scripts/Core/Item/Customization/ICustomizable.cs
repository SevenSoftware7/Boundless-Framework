namespace LandlessSkies.Core;

using System.Collections.Generic;

public interface ICustomizable : IUIObject {
	List<ICustomization> GetCustomizations();
	List<ICustomizable> GetSubCustomizables();
}