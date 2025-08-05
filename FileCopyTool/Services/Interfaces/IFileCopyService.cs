using FileCopyTool.Models;

namespace FileCopyTool.Services.Interfaces
{
	public interface IFileCopyService
	{
		(bool Success, string ErrorMessage) CopyFiles(CopyRowConfig config);
	}
}