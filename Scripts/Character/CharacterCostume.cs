using Godot;
using System;
using System.Diagnostics;
using CharacterEmotion = LandlessSkies.Core.IPortraitProvider.CharacterEmotion;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterCostume : Costume, IPortraitProvider {

    [Export] public PackedScene? ModelScene { get; private set; }
    [Export] public Texture2D? PortraitNeutral { get; private set; }
    [Export] public Texture2D? PortraitDetermined { get; private set; }
    [Export] public Texture2D? PortraitHesitant { get; private set; }
    [Export] public Texture2D? PortraitShocked { get; private set; }
    [Export] public Texture2D? PortraitDisgusted { get; private set; }
    [Export] public Texture2D? PortraitMelancholic { get; private set; }
    [Export] public Texture2D? PortraitJoyous { get; private set; }



    public Texture2D? GetPortrait(CharacterEmotion emotion) {
        return emotion switch {
            CharacterEmotion.Neutral => PortraitNeutral,
            CharacterEmotion.Determined => PortraitDetermined,
            CharacterEmotion.Hesitant => PortraitHesitant,
            CharacterEmotion.Shocked => PortraitShocked,
            CharacterEmotion.Disgusted => PortraitDisgusted,
            CharacterEmotion.Melancholic => PortraitMelancholic,
            CharacterEmotion.Joyous => PortraitJoyous,
            _ => throw new UnreachableException("Invalid CharacterEmotion value")
        };
    }

    public override CharacterModel Instantiate(Node3D root, Skeleton3D? skeleton) {
        return new CharacterModel(root, skeleton, this);
    }

}
