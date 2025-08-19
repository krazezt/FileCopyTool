using FileCopyTool.Models;
using FileCopyTool.Services.Data;

namespace FileCopyTool.Services.Interfaces
{
	public interface IConfigurationService
	{
		string CurrentLanguage { get; set; }
		SettingsConfig.PopupSettingOptions CurrentPopupSetting { get; set; }

		List<CopyRowConfig> LoadCopyConfigurations();
		SettingsConfig LoadAppSettings();
		void SaveCopyConfigurations(IEnumerable<CopyRowConfig> configs);
		void SaveAppSettings();
	}
}