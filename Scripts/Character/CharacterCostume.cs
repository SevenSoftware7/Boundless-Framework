using Godot;
using System;

namespace EndlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterCostume : Costume, IPortraitProvider {

    [Export] public PackedScene ModelScene { get; private set; }
    [Export] public Texture2D PortraitNeutral { get; private set; }
    [Export] public Texture2D PortraitDetermined { get; private set; }
    [Export] public Texture2D PortraitHesitant { get; private set; }
    [Export] public Texture2D PortraitShocked { get; private set; }
    [Export] public Texture2D PortraitDisgusted { get; private set; }
    [Export] public Texture2D PortraitMelancholic { get; private set; }
    [Export] public Texture2D PortraitJoyous { get; private set; }

    public override CharacterModel CreateModel(IModelAttachment modelAttachment) {
        return new CharacterModel(modelAttachment, this);
    }

    Texture2D IPortraitProvider.GetPortrait(IPortraitProvider.CharacterEmotion emotion) {
        return emotion switch {
            IPortraitProvider.CharacterEmotion.Neutral => PortraitNeutral,
            IPortraitProvider.CharacterEmotion.Determined => PortraitDetermined,
            IPortraitProvider.CharacterEmotion.Hesitant => PortraitHesitant,
            IPortraitProvider.CharacterEmotion.Shocked => PortraitShocked,
            IPortraitProvider.CharacterEmotion.Disgusted => PortraitDisgusted,
            IPortraitProvider.CharacterEmotion.Melancholic => PortraitMelancholic,
            IPortraitProvider.CharacterEmotion.Joyous => PortraitJoyous,
            _ => throw new NotImplementedException(),
        };
    }

}
