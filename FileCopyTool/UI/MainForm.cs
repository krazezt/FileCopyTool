using FileCopyTool.Models;
using FileCopyTool.Services.Interfaces;

namespace FileCopyTool.UI
{
	public partial class MainForm : Form
	{
		private readonly IConfigurationService configService;
		private readonly IFileCopyService fileCopyService;
		private readonly ISystemTrayService systemTrayService;
		private readonly IHotKeyService hotKeyService;
		private readonly List<CopyFileRowPanel> copyRows = [];
		private readonly Panel rowsPanel = new() { Dock = DockStyle.Fill, AutoScroll = true };

		public MainForm(
				IConfigurationService configService,
				IFileCopyService fileCopyService,
				ISystemTrayService systemTrayService,
				IHotKeyService hotKeyService)
		{
			this.configService = configService;
			this.fileCopyService = fileCopyService;
			this.systemTrayService = systemTrayService;
			this.hotKeyService = hotKeyService;
			InitializeComponent();
			this.systemTrayService.Initialize(ShowForm, ExitApplication);
			this.hotKeyService.RegisterHotKey(this.Handle);
			LoadConfigurations();
		}

		private void InitializeComponent()
		{
			Text = "File Copy Tool (Ctrl + Shift + T to perform copy)";
			FormBorderStyle = FormBorderStyle.Sizable;
			MaximizeBox = true;
			Icon = new Icon(Path.Combine(Application.StartupPath, @"Resources\Icon.ico"));
			Size = new Size(900, 400);
			AllowDrop = true;

			var mainPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 3,
				AllowDrop = true
			};
			mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

			var lblNote = new Label { Text = "※ Note: Run File Explorer as Administrator for drag-and-drop.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Height = 15 };
			var lblFrom = new Label { Text = "From\n(Multiple files accepted)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
			var lblTo = new Label { Text = "To\n(Single folder only)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };

			var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Height = 40 };
			var btnExit = new Button { Text = "Exit", Size = new Size(75, 30) };
			var btnMinimize = new Button { Text = "Minimize", Size = new Size(75, 30) };
			var btnCopy = new Button { Text = "Perform Copy", Size = new Size(120, 30) };
			var btnAddRow = new Button { Text = "Add Row", Size = new Size(75, 30) };
			buttonPanel.Controls.Add(btnExit);
			buttonPanel.Controls.Add(btnMinimize);
			buttonPanel.Controls.Add(btnCopy);
			buttonPanel.Controls.Add(btnAddRow);

			mainPanel.Controls.Add(lblNote, 0, 0);
			mainPanel.Controls.Add(lblFrom, 0, 1);
			mainPanel.Controls.Add(lblTo, 1, 1);
			mainPanel.Controls.Add(rowsPanel, 0, 2);
			mainPanel.Controls.Add(buttonPanel, 0, 3);

			mainPanel.SetColumnSpan(lblNote, 2);
			mainPanel.SetColumnSpan(rowsPanel, 2);
			mainPanel.SetColumnSpan(buttonPanel, 2);

			Controls.Add(mainPanel);

			btnCopy.Click += (s, e) => PerformCopy();
			btnMinimize.Click += (s, e) => MinimizeToTray();
			btnExit.Click += (s, e) => ExitApplication();
			btnAddRow.Click += (s, e) => AddCopyRow();
			FormClosing += (s, e) =>
			{
				if (e.CloseReason == CloseReason.UserClosing)
				{
					e.Cancel = true;
					MinimizeToTray();
				}
			};
		}

		protected override void WndProc(ref Message m)
		{
			if (hotKeyService.ProcessHotKey(m, PerformCopy) || m.Msg == 0x0400 + 1)
			{
				ShowForm();
			} else
			{
				base.WndProc(ref m);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			foreach (var row in copyRows)
			{
				row.Width = rowsPanel.Width - 5;
				row.UpdateTextBoxWidths(rowsPanel.Width);
			}
		}

		private void LoadConfigurations()
		{
			var configs = configService.LoadConfigurations();
			if (configs.Count == 0)
			{
				AddCopyRow();
				return;
			}

			for (int i = 0; i < configs.Count; i++)
			{
				var newRow = new CopyFileRowPanel(rowsPanel.Width, i == 0)
				{
					FromText = configs[i].From,
					ToText = configs[i].To,
					IsChecked = configs[i].IsChecked
				};
				newRow.Location = new Point(0, copyRows.Count * (newRow.Height + 5));
				if (i > 0)
				{
					newRow.DeleteButton.Click += (s, e) =>
					{
						copyRows.Remove(newRow);
						rowsPanel.Controls.Remove(newRow);
						UpdateRowPositions();
						SaveConfigurations();
					};
				}
				copyRows.Add(newRow);
				rowsPanel.Controls.Add(newRow);
			}
			UpdateRowPositions();
		}

		private void SaveConfigurations()
		{
			var configs = copyRows.Select(row => new CopyRowConfig
			{
				From = row.FromText,
				To = row.ToText,
				IsChecked = row.IsChecked
			}).ToList();
			configService.SaveConfigurations(configs);
		}

		private void AddCopyRow()
		{
			var newRow = new CopyFileRowPanel(rowsPanel.Width, copyRows.Count == 0);
			newRow.Location = new Point(0, copyRows.Count * (newRow.Height + 5));
			if (copyRows.Count > 0)
			{
				newRow.DeleteButton.Click += (s, e) =>
				{
					copyRows.Remove(newRow);
					rowsPanel.Controls.Remove(newRow);
					UpdateRowPositions();
					SaveConfigurations();
				};
			}
			copyRows.Add(newRow);
			rowsPanel.Controls.Add(newRow);
			UpdateRowPositions();
		}

		private void UpdateRowPositions()
		{
			for (int i = 0; i < copyRows.Count; i++)
			{
				copyRows[i].Location = new Point(0, i * (copyRows[i].Height + 5));
			}
			rowsPanel.Height = (copyRows.FirstOrDefault()?.Height + 5 ?? 50) * copyRows.Count;
		}

		private void PerformCopy()
		{
			bool hasErrors = false;
			var checkedRows = copyRows.Where(r => r.IsChecked).Select(r => new CopyRowConfig
			{
				From = r.FromText,
				To = r.ToText,
				IsChecked = r.IsChecked
			}).ToList();

			foreach (var config in checkedRows)
			{
				var (success, errorMessage) = fileCopyService.CopyFiles(config);
				if (!success)
				{
					hasErrors = true;
					MessageBox.Show($"Error copying files: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			MessageBox.Show(hasErrors ? "Copy operation completed, but some errors occurred!" : "Files copied successfully!",
				hasErrors ? "Warning" : "Success",
				MessageBoxButtons.OK,
				hasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

			SaveConfigurations();
		}

		private void MinimizeToTray()
		{
			SaveConfigurations();
			Hide();
		}

		private void ShowForm()
		{
			Show();
			WindowState = FormWindowState.Normal;
		}

		private void ExitApplication()
		{
			SaveConfigurations();
			Application.Exit();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				systemTrayService.Dispose();
				hotKeyService.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}