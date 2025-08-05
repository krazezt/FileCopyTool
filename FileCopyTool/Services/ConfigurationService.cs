using FileCopyTool.Models;
using FileCopyTool.Services.Interfaces;
using System.Text.Json;

namespace FileCopyTool.Services
{
	public class ConfigurationService : IConfigurationService
	{
		private readonly string configPath;
		private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

		public ConfigurationService()
		{
			configPath = Path.Combine(Application.StartupPath, @"Resources\config.json");
		}

		public List<CopyRowConfig> LoadConfigurations()
		{
			try
			{
				if (File.Exists(configPath))
				{
					string json = File.ReadAllText(configPath);
					var configs = JsonSerializer.Deserialize<List<CopyRowConfig>>(json);
					return configs ?? [];
				}
			} catch (Exception ex)
			{
				MessageBox.Show($"Error loading configurations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return [];
		}

		public void SaveConfigurations(IEnumerable<CopyRowConfig> configs)
		{
			try
			{
				string json = JsonSerializer.Serialize(configs, jsonSerializerOptions);
				File.WriteAllText(configPath, json);
			} catch (Exception ex)
			{
				MessageBox.Show($"Error saving configurations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}