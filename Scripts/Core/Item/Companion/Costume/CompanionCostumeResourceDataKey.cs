namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CompanionCostumeResourceDataKey : CostumeResourceDataKey {
	public CompanionCostumeResourceDataKey() : base() {}
	public CompanionCostumeResourceDataKey(string? key) : base(key) {}
}