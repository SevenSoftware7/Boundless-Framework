namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CompanionCostumeResourceItemKey : CostumeResourceItemKey {
	public CompanionCostumeResourceItemKey() : base() {}
	public CompanionCostumeResourceItemKey(string? key) : base(key) {}
}