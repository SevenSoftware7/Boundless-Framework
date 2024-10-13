namespace LandlessSkies.Core;

public interface IPersistent {
	public IPersistenceData Save();
}

public interface IPersistent<out T> : IPersistent where T : class {
	IPersistenceData IPersistent.Save() => Save();
	public new IPersistenceData<T> Save();
}