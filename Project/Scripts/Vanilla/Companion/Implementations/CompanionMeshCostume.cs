namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public sealed partial class CompanionMeshCostume : CompanionCostume, IMeshCostume {
	public override string DisplayName => _displayName;
	[Export] private string _displayName = string.Empty;

	[Export] public PackedScene? ModelScene { get; private set; }

	public override SkeletonModel Instantiate() {
		return new SkeletonModel(this);
	}
}