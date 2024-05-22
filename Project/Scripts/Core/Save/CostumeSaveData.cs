namespace LandlessSkies.Core;

using System;
using Godot;

public abstract class SceneSaveData<T> : ISaveData<T> where T : Node {
	public string ScenePath { get; set; } = string.Empty;
	public string? TypeName { get; set; }


	public SceneSaveData(T data) {
		Type baseType = typeof(Entity);
		ScenePath = data.SceneFilePath;

		if (ScenePath.Length == 0) {
			GD.PushWarning($"Attempting to save non-Scene {baseType.Name} ({data.GetPath()}) to SaveData; consider making this {baseType.Name} into a Scene to persist full functionality.");

			TypeName = data.GetType().AssemblyQualifiedName!;
		}
	}

	public virtual T? Load() {
		Type baseType = typeof(Entity);
		T? entity;

		if (ScenePath.Length == 0) {
			GD.PushWarning($"Attemping to load non-Scene {baseType.Name} ({this}) from SaveData; consider re-saving this {baseType.Name} as a Scene to persist full functionality.");

			if (TypeName is null) return null;

			Type? type = Type.GetType(TypeName);
			if (type is null) return null;


			entity = Activator.CreateInstance(type) as T;
		}
		else {
			entity = ResourceLoader.Load<PackedScene>(ScenePath)?.Instantiate<T>();
		}

		return entity;
	}
}