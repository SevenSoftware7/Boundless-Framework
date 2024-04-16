namespace LandlessSkies.Vanilla;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Godot;
using LandlessSkies.Core;
using static LandlessSkies.Core.IPortraitProvider;
using static LandlessSkies.Core.IPortraitProvider.CharacterEmotion;

[Tool]
[GlobalClass]
public abstract partial class CompanionCostume : Costume, IPortraitProvider {
	public override Texture2D? DisplayPortrait => PortraitNeutral;

	[Export] public Texture2D? PortraitNeutral { get; private set; }
	[Export] public Texture2D? PortraitDetermined { get; private set; }
	[Export] public Texture2D? PortraitHesitant { get; private set; }
	[Export] public Texture2D? PortraitShocked { get; private set; }
	[Export] public Texture2D? PortraitDisgusted { get; private set; }
	[Export] public Texture2D? PortraitMelancholic { get; private set; }
	[Export] public Texture2D? PortraitJoyous { get; private set; }

	public Texture2D? GetPortrait(CharacterEmotion emotion) => emotion switch {
		Neutral => PortraitNeutral,
		Determined => PortraitDetermined,
		Hesitant => PortraitHesitant,
		Shocked => PortraitShocked,
		Disgusted => PortraitDisgusted,
		Melancholic => PortraitMelancholic,
		Joyous => PortraitJoyous,
		_ when Enum.IsDefined(emotion) => throw new UnreachableException($"Case for {nameof(CharacterEmotion)} {emotion} not implemented."),
		_ => throw new InvalidEnumArgumentException()
	};


	public abstract override Model Instantiate();
}