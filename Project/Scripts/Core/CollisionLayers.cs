namespace LandlessSkies.Core;

public static class CollisionLayers {
	public static readonly uint Terrain = 1 << 0;
	public static readonly uint Entity = 1 << 1;
	public static readonly uint Water = 1 << 2;
	public static readonly uint Interactable = 1 << 3;
	public static readonly uint Damage = 1 << 4;
}