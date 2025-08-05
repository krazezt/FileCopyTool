namespace FileCopyTool.UI
{
	public class CopyFileRowPanel : Panel
	{
		private TextBox txtFrom;
		private TextBox txtTo;
		private Button btnBrowseFrom;
		private Button btnBrowseTo;
		private CheckBox chkEnabled;
		private Button btnDelete;

		public string FromText
		{
			get => txtFrom.Text;
			set => txtFrom.Text = value;
		}

		public string ToText
		{
			get => txtTo.Text;
			set => txtTo.Text = value;
		}

		public bool IsChecked
		{
			get => chkEnabled.Checked;
			set => chkEnabled.Checked = value;
		}

		public Button DeleteButton => btnDelete;

		public CopyFileRowPanel(int parentWidth, bool isFirstRow = false)
		{
			this.Size = new Size(parentWidth - 5, 50);
			this.BorderStyle = BorderStyle.FixedSingle;
			this.AllowDrop = true;

			chkEnabled = new CheckBox
			{
				Location = new Point(20, 5),
				Size = new Size(20, 20),
				Checked = true
			};

			btnDelete = new Button
			{
				Text = "Delete",
				Size = new Size(50, 20),
				Location = new Point(2, 25),
				Enabled = !isFirstRow
			};

			txtFrom = new TextBox
			{
				Width = parentWidth / 2 - 100,
				Location = new Point(55, 5),
				Multiline = true,
				Height = 40,
				BorderStyle = BorderStyle.Fixed3D,
				AllowDrop = true
			};

			btnBrowseFrom = new Button
			{
				Text = "Browse",
				Size = new Size(55, 40),
				Location = new Point(parentWidth / 2 - 45, 5)
			};

			txtTo = new TextBox
			{
				Width = parentWidth / 2 - 100,
				Location = new Point(parentWidth / 2 + 30, 5),
				Multiline = true,
				Height = 40,
				BorderStyle = BorderStyle.Fixed3D,
				AllowDrop = true
			};

			btnBrowseTo = new Button
			{
				Text = "Browse",
				Size = new Size(55, 40),
				Location = new Point(parentWidth - 70, 5)
			};

			// Assign event handlers
			txtFrom.DragEnter += TxtFrom_DragEnter;
			txtFrom.DragDrop += TxtFrom_DragDrop;
			txtTo.DragEnter += TxtTo_DragEnter;
			txtTo.DragDrop += TxtTo_DragDrop;
			btnBrowseFrom.Click += BtnBrowseFrom_Click;
			btnBrowseTo.Click += BtnBrowseTo_Click;

			// Reinforce AllowDrop
			txtFrom.AllowDrop = true;
			txtTo.AllowDrop = true;

			// Add controls to the panel
			this.Controls.Add(btnDelete);
			this.Controls.Add(chkEnabled);
			this.Controls.Add(txtFrom);
			this.Controls.Add(btnBrowseFrom);
			this.Controls.Add(txtTo);
			this.Controls.Add(btnBrowseTo);
		}

		public void UpdateTextBoxWidths(int parentWidth)
		{
			this.Size = new Size(parentWidth - 5, 50);
			btnDelete.Location = new Point(2, 25);
			txtFrom.Width = parentWidth / 2 - 100;
			btnBrowseFrom.Location = new Point(parentWidth / 2 - 45, 5);
			txtTo.Width = parentWidth / 2 - 100;
			txtTo.Location = new Point(parentWidth / 2 + 30, 5);
			btnBrowseTo.Location = new Point(parentWidth - 70, 5);
		}

		private void TxtFrom_DragEnter(object? sender, DragEventArgs e)
		{
			if (e.Data == null)
				return;

			// Check if the data being dragged is a file drop
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		private void TxtFrom_DragDrop(object? sender, DragEventArgs e)
		{
			if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
			{
				if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
				{
					var validFiles = files.Where(File.Exists).ToArray();
					if (validFiles.Length > 0)
					{
						txtFrom.Text = string.Join(";", validFiles);
					}
				}
			}
		}

		private void TxtTo_DragEnter(object? sender, DragEventArgs e)
		{
			if (e.Data == null)
				return;

			// Check if the data being dragged is a file drop and if it contains a single directory  
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length == 1 && Directory.Exists(files[0])) ? DragDropEffects.Copy : DragDropEffects.None;
			} else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void TxtTo_DragDrop(object? sender, DragEventArgs e)
		{
			if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
			{
				string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
				if (files != null && files.Length == 1 && Directory.Exists(files[0]))
				{
					txtTo.Text = files[0];
				}
			}
		}

		private void BtnBrowseFrom_Click(object? sender, EventArgs e)
		{
			using var ofd = new OpenFileDialog { Multiselect = true };
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				txtFrom.Text = string.Join(";", ofd.FileNames);
			}
		}

		private void BtnBrowseTo_Click(object? sender, EventArgs e)
		{
			using var fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				txtTo.Text = fbd.SelectedPath;
			}
		}
	}
}