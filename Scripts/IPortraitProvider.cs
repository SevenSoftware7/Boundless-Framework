using Godot;

namespace LandlessSkies.Core;

public interface IPortraitProvider {
    Texture2D? GetPortrait(CharacterEmotion emotion);

    
    public enum CharacterEmotion {
        Neutral,
        Determined,
        Hesitant,
        Shocked,
        Disgusted,
        Melancholic,
        Joyous
    }
}
