namespace SevenDev.Boundless;

using System.Collections.Generic;

public interface ICustomizable : IUIObject {
	Dictionary<string, ICustomization> GetCustomizations() => [];
}