
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BatchTextProcessor.Models;

namespace BatchTextProcessor.Services
{
    public class ProjectFileService
    {
        private readonly ILogger _logger;

        public ProjectFileService(ILogger logger)
        {
            _logger = logger;
        }

        public void SaveProject(string filePath, IEnumerable<TextFileItem> fileItems)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var projectData = new ProjectData
            {
                FileItems = new List<TextFileItem>(fileItems)
            };

            File.WriteAllText(filePath, JsonSerializer.Serialize(projectData, options));
            _logger.Log($"工程文件已保存: {filePath}");
        }

        public IEnumerable<TextFileItem> LoadProject(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var projectData = JsonSerializer.Deserialize<ProjectData>(json);
            _logger.Log($"工程文件已加载: {filePath}");
            return projectData?.FileItems ?? new List<TextFileItem>();
        }

        private class ProjectData
        {
            public List<TextFileItem> FileItems { get; set; } = new();
        }
    }
}
