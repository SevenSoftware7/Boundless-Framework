namespace LandlessSkies.Core;

public interface IInjectable {
	public void RequestInjection();
}

public interface IInjectable<T> : IInjectable {
	public void Inject(T value);
}