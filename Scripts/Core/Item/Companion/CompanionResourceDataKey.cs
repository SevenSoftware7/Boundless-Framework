namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CompanionResourceDataKey : GenericResourceDataKey<Companion> {
	public CompanionResourceDataKey() : base() {}
	public CompanionResourceDataKey(string? key) : base(key) {}
}