using System;
using System.ComponentModel;
using System.Diagnostics;
using Godot;
using static LandlessSkies.Core.IPortraitProvider;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
// TODO: See WeaponCostume
public abstract partial class CharacterCostume : Costume, IPortraitProvider {
    public override Texture2D? DisplayPortrait => PortraitNeutral;

    [Export] public Texture2D? PortraitNeutral { get; private set; }
	[Export] public Texture2D? PortraitDetermined { get; private set; }
	[Export] public Texture2D? PortraitHesitant { get; private set; }
	[Export] public Texture2D? PortraitShocked { get; private set; }
	[Export] public Texture2D? PortraitDisgusted { get; private set; }
	[Export] public Texture2D? PortraitMelancholic { get; private set; }
	[Export] public Texture2D? PortraitJoyous { get; private set; }

    public Texture2D? GetPortrait(CharacterEmotion emotion) => emotion switch {
		CharacterEmotion.Neutral        => PortraitNeutral,
		CharacterEmotion.Determined     => PortraitDetermined,
		CharacterEmotion.Hesitant       => PortraitHesitant,
		CharacterEmotion.Shocked        => PortraitShocked,
		CharacterEmotion.Disgusted      => PortraitDisgusted,
		CharacterEmotion.Melancholic    => PortraitMelancholic,
		CharacterEmotion.Joyous         => PortraitJoyous,
		_ when Enum.IsDefined(emotion)  => throw new UnreachableException($"Case for {nameof(CharacterEmotion)} {emotion} not implemented."),
		_                               => throw new InvalidEnumArgumentException()
	};


	public abstract override CharacterModel Instantiate();
}