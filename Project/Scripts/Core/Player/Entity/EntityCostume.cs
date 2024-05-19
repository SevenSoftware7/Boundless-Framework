namespace LandlessSkies.Core;

using Godot;
using static LandlessSkies.Core.IPortraitProvider;

[Tool]
[GlobalClass]
public partial class EntityCostume : Costume, IPortraitProvider {
	[Export] public PackedScene? ModelScene { get; private set; }

	[Export] public override string DisplayName { get; protected set; } = string.Empty;
	[Export] public override Texture2D? DisplayPortrait { get; protected set; }

	[Export] public Texture2D? PortraitDetermined { get; private set; }
	[Export] public Texture2D? PortraitHesitant { get; private set; }
	[Export] public Texture2D? PortraitShocked { get; private set; }
	[Export] public Texture2D? PortraitDisgusted { get; private set; }
	[Export] public Texture2D? PortraitMelancholic { get; private set; }
	[Export] public Texture2D? PortraitJoyous { get; private set; }


	public override Model? Instantiate() => ModelScene?.Instantiate<Model>();

	public Texture2D? GetPortrait(CharacterEmotion emotion) => Select(emotion,
		neutral: DisplayPortrait,
		determined: PortraitDetermined,
		hesitant: PortraitHesitant,
		shocked: PortraitShocked,
		disgusted: PortraitDisgusted,
		melancholic: PortraitMelancholic,
		joyous: PortraitJoyous
	);
}