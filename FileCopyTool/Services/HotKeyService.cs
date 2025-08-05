using FileCopyTool.Services.Interfaces;
using System.Runtime.InteropServices;

namespace FileCopyTool.Services
{
	public class HotKeyService : IHotKeyService
	{
		private const int MOD_CONTROL = 0x0002;
		private const int MOD_SHIFT = 0x0004;
		private const int VK_T = 0x54;
		private const int HOTKEY_ID = 1;
		private bool isDisposed;
		private IntPtr hWnd;

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		public void RegisterHotKey(IntPtr hWnd)
		{
			this.hWnd = hWnd;
			RegisterHotKey(hWnd, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_T);
		}

		public bool ProcessHotKey(Message m, Action hotKeyAction)
		{
			if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID)
			{
				hotKeyAction();
				return true;
			}
			return false;
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				UnregisterHotKey(hWnd, HOTKEY_ID);
				isDisposed = true;
			}
		}
	}
}