using System.Collections.Generic;

namespace LandlessSkies.Core;

public interface ICustomizable : IUIObject {
	List<ICustomization> GetCustomizations();
	List<ICustomizable> GetSubCustomizables();
}