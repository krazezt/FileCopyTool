using FileCopyTool.Services.Data;

namespace FileCopyTool.Models
{
	public class SettingsConfig
	{
		public enum PopupSettingOptions
		{
			All = 0,
			WarningOrError = 1,
			ErrorOnly = 2,
			Never = 3
		}

		public string Language { get; set; } = LanguageResources.DefaultLanguage;
		public PopupSettingOptions PopupSetting { get; set; } = PopupSettingOptions.All;
	}
}