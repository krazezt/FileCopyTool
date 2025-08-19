using FileCopyTool.Models;
using FileCopyTool.Services.Interfaces;
using System.Text.Json;
using System.Globalization;
using FileCopyTool.Services.Data;

namespace FileCopyTool.Services
{
	public class ConfigurationService : IConfigurationService
	{
		private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
		private readonly string copyConfigPath = Path.Combine(Application.StartupPath, @"Resources\copy-config.json");
		private readonly string settingsConfigPath = Path.Combine(Application.StartupPath, @"Resources\settings-config.json");

		public string CurrentLanguage { get; set; } = LanguageResources.DefaultLanguage;
		public SettingsConfig.PopupSettingOptions CurrentPopupSetting { get; set; } = SettingsConfig.PopupSettingOptions.All;

		public ConfigurationService()
		{
		}

		public List<CopyRowConfig> LoadCopyConfigurations()
		{
			try
			{
				if (File.Exists(copyConfigPath))
				{
					string json = File.ReadAllText(copyConfigPath);
					var configs = JsonSerializer.Deserialize<List<CopyRowConfig>>(json);
					return configs ?? [];
				}
			} catch (Exception ex)
			{
				if (CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.ErrorOnly)
					MessageBox.Show(
						string.Format(LanguageResources.GetString("MessageConfigError", LoadAppSettings().Language), "loading", ex.Message),
						LanguageResources.GetString("MessageError", LoadAppSettings().Language),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
			}
			return [];
		}

		public void SaveCopyConfigurations(IEnumerable<CopyRowConfig> configs)
		{
			try
			{
				string json = JsonSerializer.Serialize(configs, jsonSerializerOptions);
				File.WriteAllText(copyConfigPath, json);
			} catch (Exception ex)
			{
				if (CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.ErrorOnly)
					MessageBox.Show(
						string.Format(LanguageResources.GetString("MessageConfigError", LoadAppSettings().Language), "saving", ex.Message),
						LanguageResources.GetString("MessageError", LoadAppSettings().Language),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
			}
		}

		public SettingsConfig LoadAppSettings()
		{
			try
			{
				if (File.Exists(settingsConfigPath))
				{
					string json = File.ReadAllText(settingsConfigPath);
					var config = JsonSerializer.Deserialize<SettingsConfig>(json);
					CurrentLanguage = config?.Language ?? LanguageResources.DefaultLanguage;
					CurrentPopupSetting = config?.PopupSetting ?? SettingsConfig.PopupSettingOptions.All;
					return config ?? new SettingsConfig();
				}
			} catch (Exception ex)
			{
				if (CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.ErrorOnly)
					MessageBox.Show(
						string.Format(LanguageResources.GetString("MessageConfigError", "EN"), "loading app", ex.Message),
						LanguageResources.GetString("MessageError", "EN"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
			}
			return new SettingsConfig();
		}

		public void SaveAppSettings()
		{
			try
			{
				string json = JsonSerializer.Serialize(new SettingsConfig()
				{
					Language = CurrentLanguage,
					PopupSetting = CurrentPopupSetting
				}, jsonSerializerOptions);
				File.WriteAllText(settingsConfigPath, json);
			} catch (Exception ex)
			{
				if (CurrentPopupSetting <= SettingsConfig.PopupSettingOptions.ErrorOnly)
					MessageBox.Show(
						string.Format(LanguageResources.GetString("MessageConfigError", LoadAppSettings().Language), "saving app", ex.Message),
						LanguageResources.GetString("MessageError", LoadAppSettings().Language),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
			}
		}
	}
}