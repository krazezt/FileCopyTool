using FileCopyTool.Models;

namespace FileCopyTool.Services.Interfaces
{
    public interface IConfigurationService
    {
        List<CopyRowConfig> LoadConfigurations();
        void SaveConfigurations(IEnumerable<CopyRowConfig> configs);
    }
}