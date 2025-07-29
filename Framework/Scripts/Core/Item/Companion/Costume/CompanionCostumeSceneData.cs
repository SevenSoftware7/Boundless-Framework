namespace Seven.Boundless;

using Godot;
using static Seven.Boundless.IPortraitProvider;

[Tool]
[GlobalClass]
public partial class CompanionCostumeSceneData : SceneCostumeData, IPortraitProvider {
	[ExportGroup("Portraits")]
	[Export] public Texture2D? PortraitDetermined { get; private set; }
	[Export] public Texture2D? PortraitHesitant { get; private set; }
	[Export] public Texture2D? PortraitShocked { get; private set; }
	[Export] public Texture2D? PortraitDisgusted { get; private set; }
	[Export] public Texture2D? PortraitMelancholic { get; private set; }
	[Export] public Texture2D? PortraitJoyous { get; private set; }


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