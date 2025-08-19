using FileCopyTool.Models;
using FileCopyTool.Services.Data;
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
		private readonly MenuStrip toolBar = new();
		private readonly FormControls formControls = new();
		private readonly Panel rowsPanel = new() { Dock = DockStyle.Fill, AutoScroll = true };

		public MainForm(
				IConfigurationService configService,
				IFileCopyService fileCopyService,
				ISystemTrayService systemTrayService,
				IHotKeyService hotKeyService)
		{
			configService.LoadAppSettings();
			this.configService = configService;
			this.fileCopyService = fileCopyService;
			this.systemTrayService = systemTrayService;
			this.hotKeyService = hotKeyService;
			InitializeComponent();
			this.systemTrayService.Initialize(ShowForm, ExitApplication);
			this.hotKeyService.RegisterHotKey(this.Handle);
			this.Controls.Add(toolBar);
			LoadConfigurations();
		}

		private void InitializeComponent()
		{
			Text = LanguageResources.GetString("FormTitle", configService.CurrentLanguage);
			FormBorderStyle = FormBorderStyle.Sizable;
			MaximizeBox = true;
			Icon = new Icon(Path.Combine(Application.StartupPath, @"Resources\Icon.ico"));
			Size = new Size(900, 400);
			AllowDrop = true;

			var mainPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 4,
				AllowDrop = true
			};
			mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 15F));  // Note label
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // From - To label panel
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Rows panel
			mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // Button panel

			ToolStripMenuItem toolsMenu = new(LanguageResources.GetString("ToolsMenu", configService.CurrentLanguage));
			ToolStripMenuItem settingsMenu = new(LanguageResources.GetString("SettingsMenu", configService.CurrentLanguage));
			SetupToolsMenu(toolsMenu);

			ToolStripMenuItem popupMenu = new(LanguageResources.GetString("PopupMenu", configService.CurrentLanguage));
			ToolStripMenuItem languageMenu = new(LanguageResources.GetString("LanguageMenu", configService.CurrentLanguage));
			SetupPopupMenu(popupMenu);
			SetupLanguageMenu(languageMenu);

			settingsMenu.DropDownItems.Add(popupMenu);
			settingsMenu.DropDownItems.Add(languageMenu);

			toolBar.Items.Add(toolsMenu);
			toolBar.Items.Add(new ToolStripSeparator());
			toolBar.Items.Add(settingsMenu);
			toolBar.Items.Add(new ToolStripSeparator());
			
			var lblNote = new Label { Text = LanguageResources.GetString("LabelNote", configService.CurrentLanguage), Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, Height = 15 };
			var lblFrom = new Label { Text = LanguageResources.GetString("LabelFrom", configService.CurrentLanguage), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
			var lblTo = new Label { Text = LanguageResources.GetString("LabelTo", configService.CurrentLanguage), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };

			var languagePanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
			var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Height = 40 };
			var btnExit = new Button { Text = LanguageResources.GetString("ButtonExit", configService.CurrentLanguage), Size = new Size(75, 30) };
			var btnMinimize = new Button { Text = LanguageResources.GetString("ButtonMinimize", configService.CurrentLanguage), Size = new Size(75, 30) };
			var btnCopy = new Button { Text = LanguageResources.GetString("ButtonCopy", configService.CurrentLanguage), Size = new Size(120, 30) };
			var btnAddRow = new Button { Text = LanguageResources.GetString("ButtonAddRow", configService.CurrentLanguage), Size = new Size(75, 30) };

			formControls.toolsMenu = toolsMenu;
			formControls.settingsMenu = settingsMenu;
			formControls.popupMenu = popupMenu;
			formControls.languageMenu = languageMenu;
			formControls.lblNote = lblNote;
			formControls.lblFrom = lblFrom;
			formControls.lblTo = lblTo;
			formControls.btnExit = btnExit;
			formControls.btnMinimize = btnMinimize;
			formControls.btnCopy = btnCopy;
			formControls.btnAddRow = btnAddRow;

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
			FormClosing += (s, e) => CustomFormClosed(s, e);

			UpdateUIText();
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
			var configs = configService.LoadCopyConfigurations();
			if (configs.Count == 0)
			{
				AddCopyRow();
				return;
			}

			for (int i = 0; i < configs.Count; i++)
			{
				var newRow = new CopyFileRowPanel(rowsPanel.Width, configService.CurrentLanguage, i == 0)
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
			
			configService.SaveCopyConfigurations(configs);
		}

		private void ChangePopupSetting(SettingsConfig.PopupSettingOptions popupSettingOption)
		{
			configService.CurrentPopupSetting = popupSettingOption;
			configService.SaveAppSettings();
			SaveConfigurations();
			UpdateUIText();
		}

		private void ChangeLanguage(string lang)
		{
			configService.CurrentLanguage = lang;
			configService.SaveAppSettings();
			SaveConfigurations();
			UpdateUIText();
		}

		private void SetupToolsMenu(ToolStripMenuItem toolsMenu)
		{
			ToolStripMenuItem addRowMenu = new(LanguageResources.GetString("ButtonAddRow", configService.CurrentLanguage));
			ToolStripMenuItem performCopyMenu = new(LanguageResources.GetString("ButtonCopy", configService.CurrentLanguage));
			ToolStripMenuItem minimizeMenu = new(LanguageResources.GetString("ButtonMinimize", configService.CurrentLanguage));
			ToolStripMenuItem exitMenu = new(LanguageResources.GetString("ButtonExit", configService.CurrentLanguage));

			addRowMenu.Click += (s, e) => AddCopyRow();
			performCopyMenu.Click += (s, e) => PerformCopy();
			minimizeMenu.Click += (s, e) => MinimizeToTray();
			exitMenu.Click += (s, e) => ExitApplication();

			toolsMenu.DropDownItems.Add(addRowMenu);
			toolsMenu.DropDownItems.Add(performCopyMenu);
			toolsMenu.DropDownItems.Add(minimizeMenu);
			toolsMenu.DropDownItems.Add(exitMenu);

			formControls.addRowMenu = addRowMenu;
			formControls.performCopyMenu = performCopyMenu;
			formControls.minimizeMenu = minimizeMenu;
			formControls.exitMenu = exitMenu;
		}

		private void SetupPopupMenu(ToolStripMenuItem popupMenu)
		{
			ToolStripMenuItem popupSettingAll = new(LanguageResources.GetString("PopupSettingAll", configService.CurrentLanguage))
			{
				CheckOnClick = true,
				Checked = (configService.CurrentPopupSetting == SettingsConfig.PopupSettingOptions.All)
			};

			ToolStripMenuItem popupSettingWarningOrError = new(LanguageResources.GetString("PopupSettingWarningOrError", configService.CurrentLanguage))
			{
				CheckOnClick = true,
				Checked = (configService.CurrentPopupSetting == SettingsConfig.PopupSettingOptions.WarningOrError)
			};

			ToolStripMenuItem popupSettingErrorOnly = new(LanguageResources.GetString("PopupSettingErrorOnly", configService.CurrentLanguage))
			{
				CheckOnClick = true,
				Checked = (configService.CurrentPopupSetting == SettingsConfig.PopupSettingOptions.ErrorOnly)
			};

			ToolStripMenuItem popupSettingNever = new(LanguageResources.GetString("PopupSettingNever", configService.CurrentLanguage))
			{
				CheckOnClick = true,
				Checked = (configService.CurrentPopupSetting == SettingsConfig.PopupSettingOptions.Never)
			};

			popupSettingAll.Click += (s, e) =>
			{
				foreach (ToolStripMenuItem item in popupMenu.DropDownItems)
					item.Checked = false;

				popupSettingAll.Checked = true;
				ChangePopupSetting(SettingsConfig.PopupSettingOptions.All);
			};

			popupSettingWarningOrError.Click += (s, e) =>
			{
				foreach (ToolStripMenuItem item in popupMenu.DropDownItems)
					item.Checked = false;

				popupSettingWarningOrError.Checked = true;
				ChangePopupSetting(SettingsConfig.PopupSettingOptions.WarningOrError);
			};

			popupSettingErrorOnly.Click += (s, e) =>
			{
				foreach (ToolStripMenuItem item in popupMenu.DropDownItems)
					item.Checked = false;

				popupSettingErrorOnly.Checked = true;
				ChangePopupSetting(SettingsConfig.PopupSettingOptions.ErrorOnly);
			};

			popupSettingNever.Click += (s, e) =>
			{
				foreach (ToolStripMenuItem item in popupMenu.DropDownItems)
					item.Checked = false;

				popupSettingNever.Checked = true;
				ChangePopupSetting(SettingsConfig.PopupSettingOptions.Never);
			};

			popupMenu.DropDownItems.Add(popupSettingAll);
			popupMenu.DropDownItems.Add(popupSettingWarningOrError);
			popupMenu.DropDownItems.Add(popupSettingErrorOnly);
			popupMenu.DropDownItems.Add(popupSettingNever);

			formControls.popupSettingAll = popupSettingAll;
			formControls.popupSettingWarningOrError = popupSettingWarningOrError;
			formControls.popupSettingErrorOnly = popupSettingErrorOnly;
			formControls.popupSettingNever = popupSettingNever;
		}

		private void SetupLanguageMenu(ToolStripMenuItem languageMenu)
		{
			foreach (string lang in LanguageResources.SupportedLanguageTitles)
			{
				ToolStripMenuItem langItem = new(lang)
				{
					CheckOnClick = true,
					Checked = (lang == configService.CurrentLanguage)
				};

				langItem.Click += (s, e) =>
				{
					foreach (ToolStripMenuItem item in languageMenu.DropDownItems)
						item.Checked = false;

					langItem.Checked = true;
					ChangeLanguage(LanguageResources.SupportedLanguages[Array.IndexOf(LanguageResources.SupportedLanguageTitles, lang)]);
				};

				languageMenu.DropDownItems.Add(langItem);
			}
		}

		private void AddCopyRow()
		{
			var newRow = new CopyFileRowPanel(rowsPanel.Width, configService.CurrentLanguage, copyRows.Count == 0);
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

		private void UpdateUIText()
		{
			Text = LanguageResources.GetString("FormTitle", configService.CurrentLanguage);
			formControls.toolsMenu.Text = LanguageResources.GetString("ToolsMenu", configService.CurrentLanguage);
			formControls.addRowMenu.Text = LanguageResources.GetString("ButtonAddRow", configService.CurrentLanguage);
			formControls.performCopyMenu.Text = LanguageResources.GetString("ButtonCopy", configService.CurrentLanguage);
			formControls.minimizeMenu.Text = LanguageResources.GetString("ButtonMinimize", configService.CurrentLanguage);
			formControls.exitMenu.Text = LanguageResources.GetString("ButtonExit", configService.CurrentLanguage);
			formControls.settingsMenu.Text = LanguageResources.GetString("SettingsMenu", configService.CurrentLanguage);
			formControls.popupSettingAll.Text = LanguageResources.GetString("PopupSettingAll", configService.CurrentLanguage);
			formControls.popupSettingWarningOrError.Text = LanguageResources.GetString("PopupSettingWarningOrError", configService.CurrentLanguage);
			formControls.popupSettingErrorOnly.Text = LanguageResources.GetString("PopupSettingErrorOnly", configService.CurrentLanguage);
			formControls.popupSettingNever.Text = LanguageResources.GetString("PopupSettingNever", configService.CurrentLanguage);
			formControls.popupMenu.Text = LanguageResources.GetString("PopupMenu", configService.CurrentLanguage);
			formControls.languageMenu.Text = LanguageResources.GetString("LanguageMenu", configService.CurrentLanguage);
			formControls.lblNote.Text = LanguageResources.GetString("LabelNote", configService.CurrentLanguage);
			formControls.lblFrom.Text = LanguageResources.GetString("LabelFrom", configService.CurrentLanguage);
			formControls.lblTo.Text = LanguageResources.GetString("LabelTo", configService.CurrentLanguage);
			formControls.btnExit.Text = LanguageResources.GetString("ButtonExit", configService.CurrentLanguage);
			formControls.btnMinimize.Text = LanguageResources.GetString("ButtonMinimize", configService.CurrentLanguage);
			formControls.btnCopy.Text = LanguageResources.GetString("ButtonCopy", configService.CurrentLanguage);
			formControls.btnAddRow.Text = LanguageResources.GetString("ButtonAddRow", configService.CurrentLanguage);
			foreach (var row in copyRows)
			{
				row.BtnBrowseFrom.Text = LanguageResources.GetString("ButtonBrowse", configService.CurrentLanguage);
				row.BtnBrowseTo.Text = LanguageResources.GetString("ButtonBrowse", configService.CurrentLanguage);
				row.BtnDelete.Text = LanguageResources.GetString("ButtonDelete", configService.CurrentLanguage);
			}
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
					if (configService.CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.ErrorOnly)
						MessageBox.Show(
							string.Format(LanguageResources.GetString("MessageCopyError", configService.CurrentLanguage), errorMessage),
							LanguageResources.GetString("MessageError", configService.CurrentLanguage),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
			}

			if (hasErrors)
			{
				if (configService.CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.WarningOrError)
				{
					MessageBox.Show(
						LanguageResources.GetString("MessageCopyWarning", configService.CurrentLanguage),
						LanguageResources.GetString("MessageWarning", configService.CurrentLanguage),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
			} else
			{
				if (configService.CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.All)
					MessageBox.Show(
						LanguageResources.GetString("MessageCopySuccess", configService.CurrentLanguage),
						LanguageResources.GetString("MessageSuccess", configService.CurrentLanguage),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
			}

			SaveConfigurations();
		}

		private void MinimizeToTray()
		{
			SaveConfigurations();
			Hide();
		}

		private void CustomFormClosed(object? sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				MinimizeToTray();
			}
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

	public class FormControls
	{
		public ToolStripMenuItem toolsMenu;
		public ToolStripMenuItem addRowMenu;
		public ToolStripMenuItem performCopyMenu;
		public ToolStripMenuItem minimizeMenu;
		public ToolStripMenuItem settingsMenu;
		public ToolStripMenuItem exitMenu;
		public ToolStripMenuItem popupMenu;
		public ToolStripMenuItem popupSettingAll;
		public ToolStripMenuItem popupSettingWarningOrError;
		public ToolStripMenuItem popupSettingErrorOnly;
		public ToolStripMenuItem popupSettingNever;
		public ToolStripMenuItem languageMenu;
		public Label lblNote;
		public Label lblFrom;
		public Label lblTo;
		public Button btnExit;
		public Button btnMinimize;
		public Button btnCopy;
		public Button btnAddRow;

		public FormControls()
		{
			toolsMenu = new ToolStripMenuItem();
			addRowMenu = new ToolStripMenuItem();
			performCopyMenu = new ToolStripMenuItem();
			minimizeMenu = new ToolStripMenuItem();
			exitMenu = new ToolStripMenuItem();
			settingsMenu = new ToolStripMenuItem();
			popupMenu = new ToolStripMenuItem();
			popupSettingAll = new ToolStripMenuItem();
			popupSettingWarningOrError = new ToolStripMenuItem();
			popupSettingErrorOnly = new ToolStripMenuItem();
			popupSettingNever = new ToolStripMenuItem();
			languageMenu = new ToolStripMenuItem();
			lblNote = new Label();
			lblFrom = new Label();
			lblTo = new Label();
			btnExit = new Button();
			btnMinimize = new Button();
			btnCopy = new Button();
			btnAddRow = new Button();
		}
	}
}