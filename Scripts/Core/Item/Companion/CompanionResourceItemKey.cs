namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class CompanionResourceItemKey : GenericResourceItemKey<Companion> {
	public CompanionResourceItemKey() : base() {}
	public CompanionResourceItemKey(string? key) : base(key) {}
}