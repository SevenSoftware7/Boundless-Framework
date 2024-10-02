namespace LandlessSkies.Core;

using System;
using System.Diagnostics.CodeAnalysis;
using Godot;

public abstract class SceneSaveData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T> : ISaveData<T> where T : Node {
	public string ScenePath = string.Empty;

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public Type Type;


	public SceneSaveData(T data) {
		Type = typeof(T);
		ScenePath = data.SceneFilePath;

		if (ScenePath.Length == 0) {
			GD.PushWarning($"Attempting to save non-Scene {Type.Name} ({data.GetPath()}) to SaveData; consider making this {Type.Name} into a Scene to persist full functionality.");
		}
	}

	public virtual T? Load() {
		T? entity;

		if (ScenePath.Length == 0) {
			GD.PushWarning($"Attemping to load non-Scene {Type.Name} ({this}) from SaveData; consider re-saving this {Type.Name} as a Scene to persist full functionality.");
			entity = Activator.CreateInstance(Type) as T;
		}
		else {
			entity = ResourceLoader.Load<PackedScene>(ScenePath)?.Instantiate<T>();
		}

		return entity;
	}
}