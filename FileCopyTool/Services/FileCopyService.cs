using FileCopyTool.Models;
using FileCopyTool.Services.Interfaces;

namespace FileCopyTool.Services
{
	public class FileCopyService : IFileCopyService
	{
		public (bool Success, string ErrorMessage) CopyFiles(CopyRowConfig config)
		{
			try
			{
				string[] fromFiles = config.From.Replace("\"", "").Split([";", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries);
				string toPath = config.To.Replace("\"", "").Trim();

				if (!Directory.Exists(toPath))
				{
					Directory.CreateDirectory(toPath);
				}

				foreach (string fromFile in fromFiles)
				{
					string fileName = Path.GetFileName(fromFile.Trim());
					string destPath = Path.Combine(toPath, fileName);
					File.Copy(fromFile.Trim(), destPath, true);
				}
				return (true, string.Empty);
			} catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}
	}
}