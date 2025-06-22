namespace SevenDev.Boundless;

public static class CollisionLayers {
	public static readonly uint Terrain = 1 << 0;
	public static readonly uint Water = 1 << 1;

	public static readonly uint Entity = 1 << 8;
	public static readonly uint Prop = 1 << 9;
	public static readonly uint Interactable = 1 << 10;
	public static readonly uint Damage = 1 << 11;
}