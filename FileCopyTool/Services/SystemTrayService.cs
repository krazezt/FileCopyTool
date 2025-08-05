using FileCopyTool.Services.Interfaces;

namespace FileCopyTool.Services
{
	public class SystemTrayService : ISystemTrayService
	{
		private readonly NotifyIcon trayIcon = new() { Icon = LoadTrayIcon(), Visible = true, Text = "File Copy Tool" };
		private bool isDisposed;

		public void Initialize(Action restoreAction, Action exitAction)
		{
			var trayMenu = new ContextMenuStrip();
			trayMenu.Items.Add("Open", null, (s, e) => restoreAction());
			trayMenu.Items.Add("Exit", null, (s, e) => exitAction());
			trayIcon.ContextMenuStrip = trayMenu;
			trayIcon.DoubleClick += (s, e) => restoreAction();
		}

		private static Icon LoadTrayIcon()
		{
			try
			{
				string iconPath = Path.Combine(Application.StartupPath, @"Resources\Icon.ico");
				if (File.Exists(iconPath))
					return new Icon(iconPath);
				Console.WriteLine("Icon path not found!");
				return SystemIcons.Application;
			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return SystemIcons.Application;
			}
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				trayIcon?.Dispose();
				isDisposed = true;
			}
		}
	}
}