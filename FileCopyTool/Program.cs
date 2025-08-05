using System.Runtime.InteropServices;
using FileCopyTool.Services;
using FileCopyTool.UI;

namespace FileCopyTool
{
	static class Program
	{
		private const string MutexName = "FileCopyTool_MutexInstance_Krazezt";
		private const string WindowName = "File Copy Tool";

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		private const int WM_USER = 0x0400;
		private const int WM_RESTORE_APP = WM_USER + 1;

		[STAThread]
		static void Main()
		{
			bool createdNew;
			using (Mutex mutex = new(true, MutexName, out createdNew))
			{
				if (createdNew)
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					var configService = new ConfigurationService();
					var fileCopyService = new FileCopyService();
					var systemTrayService = new SystemTrayService();
					var hotKeyService = new HotKeyService();
					Application.Run(new MainForm(configService, fileCopyService, systemTrayService, hotKeyService));
				} else
				{
					MessageBox.Show("Another instance of FileCopyTool is already running.", "Instance Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
					IntPtr hWnd = FindWindow(null, WindowName);
					if (hWnd != IntPtr.Zero)
					{
						PostMessage(hWnd, WM_RESTORE_APP, IntPtr.Zero, IntPtr.Zero);
					}
				}
			}
		}
	}
}