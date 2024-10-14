namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class DataKey : Resource {
	[Export] public StringName String {
		get => _string;
		private set => _string = value.ToString().ToSnakeCase();
	}
	private StringName _string = string.Empty;


	public DataKey() : this(null) { }
	public DataKey(string? @string = null) : base() {
		String = @string ?? string.Empty;
	}
}