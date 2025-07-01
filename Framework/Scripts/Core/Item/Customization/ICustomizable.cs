namespace SevenDev.Boundless;

using System.Collections.Generic;

public interface ICustomizable : IUIObject {
	public Dictionary<string, ICustomization> GetCustomizations() => [];
}