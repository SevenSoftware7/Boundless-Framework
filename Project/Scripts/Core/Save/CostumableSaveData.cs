// namespace LandlessSkies.Core;

// using Godot;
// using SevenDev.Utility;


// public abstract class CostumableSaveData<T>(T data, CostumeHolder? costumeHolder) : SceneSaveData<T>(data) where T : Node {
// 	public string? CostumePath = costumeHolder?.Costume?.ResourcePath;

// 	public override T? Load() {
// 		if (base.Load() is not T data) return null;

// 		if (CostumePath is not null) {
// 			Costume? costume = ResourceLoader.Load<Costume>(CostumePath);
// 			new CostumeHolder(costume).ParentTo(data);
// 		}

// 		return data;
// 	}
// }