namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CompanionData : Resource, IUIObject {
	[Export] public CompanionCostume? BaseCostume { get; private set; }

	[Export] public string DisplayName { get; private set; } = "";
	public Texture2D? DisplayPortrait => BaseCostume?.DisplayPortrait;


	public virtual Companion Instantiate(CompanionCostume? costume = null) =>
		new(this, costume ?? BaseCostume);
}