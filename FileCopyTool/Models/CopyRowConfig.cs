namespace FileCopyTool.Models
{
	public class CopyRowConfig
	{
		public required string From { get; set; }
		public required string To { get; set; }
		public bool IsChecked { get; set; }
	}
}