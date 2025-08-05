using System;

namespace FileCopyTool.Services.Interfaces
{
	public interface ISystemTrayService : IDisposable
	{
		void Initialize(Action restoreAction, Action exitAction);
	}
}