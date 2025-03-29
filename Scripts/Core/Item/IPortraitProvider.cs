namespace LandlessSkies.Core;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Godot;

public interface IPortraitProvider {
	public static Texture2D? Select(
		CharacterEmotion emotion,
		Texture2D? neutral = null,
		Texture2D? determined = null,
		Texture2D? hesitant = null,
		Texture2D? shocked = null,
		Texture2D? disgusted = null,
		Texture2D? melancholic = null,
		Texture2D? joyous = null
	) => emotion switch {
		CharacterEmotion.Neutral => neutral,
		CharacterEmotion.Determined => determined ?? neutral,
		CharacterEmotion.Hesitant => hesitant ?? neutral,
		CharacterEmotion.Shocked => shocked ?? neutral,
		CharacterEmotion.Disgusted => disgusted ?? neutral,
		CharacterEmotion.Melancholic => melancholic ?? neutral,
		CharacterEmotion.Joyous => joyous ?? neutral,
		_ when Enum.IsDefined(emotion) => throw new UnreachableException($"Case for {nameof(CharacterEmotion)} {emotion} not implemented."),
		_ => throw new InvalidEnumArgumentException()
	};


	public Texture2D? GetPortrait(CharacterEmotion emotion);



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
