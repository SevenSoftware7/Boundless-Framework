namespace LandlessSkies.Core;

using System.Collections.Generic;

public interface ICustomizable : IUIObject {
	Dictionary<string, ICustomization> GetCustomizations() => [];
}