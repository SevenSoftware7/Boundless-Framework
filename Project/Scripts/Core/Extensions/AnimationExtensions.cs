namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;

public static class AnimationExtensions {
	public static IEnumerable<Animation> GetAnimations(this AnimationPlayer animationPlayer) {
		return animationPlayer
			.GetAnimationList()
			.Select(n => animationPlayer.GetAnimation(n));
	}

	public static IEnumerable<AnimationTrack> GetTracks(this Animation animation) {
		int count = animation.GetTrackCount();
		for (int i = 0; i < count; i++) {
			yield return new AnimationTrack(animation, i);
		}
	}
}

public struct AnimationTrack(Animation animation, int id) {
	public readonly Animation Animation = animation;
	public int TrackIdx { get; private set; } = id;


	public readonly IEnumerable<AnimationKey> GetKeys() {
		int count = Animation.TrackGetKeyCount(TrackIdx);
		for (int i = 0; i < count; i++) {
			yield return new AnimationKey(this, i);
		}
	}

	public readonly int GetKeyCount() =>
		Animation.TrackGetKeyCount(TrackIdx);

	public readonly NodePath GetPath() =>
		Animation.TrackGetPath(TrackIdx);
	public readonly void SetPath(NodePath path) =>
		Animation.TrackSetPath(TrackIdx, path);

	public readonly Animation.TrackType GetTrackType() =>
		Animation.TrackGetType(TrackIdx);

	public readonly bool IsCompressed() =>
		Animation.TrackIsCompressed(TrackIdx);

	public readonly bool IsEnabled() =>
		Animation.TrackIsEnabled(TrackIdx);
	public readonly void SetEnabled(bool enabled) =>
		Animation.TrackSetEnabled(TrackIdx, enabled);

	public readonly bool IsImported() =>
		Animation.TrackIsImported(TrackIdx);
	public readonly void SetImported(bool imported) =>
		Animation.TrackSetImported(TrackIdx, imported);


	public void MoveDown() {
		Animation.TrackMoveDown(TrackIdx);
		TrackIdx++;
	}
	public void MoveUp() {
		Animation.TrackMoveUp(TrackIdx);
		TrackIdx--;
	}
	public void MoveTo(int toIdx) {
		Animation.TrackMoveTo(TrackIdx, toIdx);
		TrackIdx = toIdx;
	}
	public void Swap(int withIdx) {
		Animation.TrackSwap(TrackIdx, withIdx);
		TrackIdx = withIdx;
	}


	public readonly int FindKey(double time, Animation.FindMode findMode = Animation.FindMode.Nearest) =>
		Animation.TrackFindKey(TrackIdx, time, findMode);
	public readonly int InsertKey(double time, Variant key, float transition = 1) =>
		Animation.TrackInsertKey(TrackIdx, time, key, transition);
	public readonly void RemoveKey(int keyIdx) =>
		Animation.TrackRemoveKey(TrackIdx, keyIdx);
	public readonly void RemoveKeyAtTime(double time) =>
		Animation.TrackRemoveKeyAtTime(TrackIdx, time);

	public readonly void SetInterpolationLoopWrap(bool interpolation) =>
		Animation.TrackSetInterpolationLoopWrap(TrackIdx, interpolation);
	public readonly void SetInterpolationType(Animation.InterpolationType interpolation) =>
		Animation.TrackSetInterpolationType(TrackIdx, interpolation);
}

public readonly struct AnimationKey(AnimationTrack track, int id) {
	public readonly AnimationTrack Track = track;
	public readonly int KeyIdx = id;

	public double GetTime() =>
		Track.Animation.TrackGetKeyTime(Track.TrackIdx, KeyIdx);
	public void SetTime(double time) =>
		Track.Animation.TrackSetKeyTime(KeyIdx, KeyIdx, time);

	public Variant GetValue() =>
		Track.Animation.TrackGetKeyValue(Track.TrackIdx, KeyIdx);
	public void SetValue(Variant value) =>
		Track.Animation.TrackSetKeyValue(KeyIdx, KeyIdx, value);

	public float GetTransition() =>
		Track.Animation.TrackGetKeyTransition(Track.TrackIdx, KeyIdx);
	public void SetTransition(float transition) =>
		Track.Animation.TrackSetKeyTransition(KeyIdx, KeyIdx, transition);
}