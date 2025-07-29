namespace Seven.Boundless.Modding;

using Seven.Boundless.Persistence;


public interface IModInterface {
	public IItemDataProvider? ItemDataProvider { get; }

	public void Start();
	public void Stop();
}