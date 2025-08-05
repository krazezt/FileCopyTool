namespace FileCopyTool.Services.Interfaces
{
	public interface IHotKeyService : IDisposable
	{
		void RegisterHotKey(nint hWnd);
		bool ProcessHotKey(Message m, Action hotKeyAction);
	}
}