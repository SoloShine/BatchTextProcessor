
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BatchTextProcessor.Services
{
    public class FileMergeService
    {
        private readonly ILogger _logger;

        public FileMergeService(ILogger logger)
        {
            _logger = logger;
        }

        public bool MergeFiles(IEnumerable<string> filePaths, string outputPath, string mergedName)
        {
            try
            {
                var contentBuilder = new StringBuilder();
                foreach (var filePath in filePaths)
                {
                    contentBuilder.AppendLine($"// ====== 来源文件: {Path.GetFileName(filePath)} ======");
                    contentBuilder.AppendLine(File.ReadAllText(filePath));
                    contentBuilder.AppendLine(); // 添加文件分隔空行
                }

                Directory.CreateDirectory(outputPath); // 确保输出目录存在
                File.WriteAllText(Path.Combine(outputPath, $"{mergedName}.txt"), contentBuilder.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"合并文件失败: {ex.Message}\n文件列表: {string.Join(", ", filePaths)}");
                return false;
            }
        }

        public bool CopyFiles(IEnumerable<string> filePaths, string outputPath)
        {
            try
            {
                Directory.CreateDirectory(outputPath); // 确保输出目录存在
                foreach (var filePath in filePaths)
                {
                    File.Copy(filePath, Path.Combine(outputPath, Path.GetFileName(filePath)), overwrite: true);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"复制文件失败: {ex.Message}\n文件列表: {string.Join(", ", filePaths)}");
                return false;
            }
        }
    }
}
