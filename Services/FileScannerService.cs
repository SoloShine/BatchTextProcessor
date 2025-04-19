
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatchTextProcessor.Services
{
    public class FileScannerService
    {
        public IEnumerable<string> ScanTextFiles(string folderPath)
        {
            return Directory.EnumerateFiles(folderPath, "*.txt", SearchOption.AllDirectories);
        }

        public string FindCommonPrefix(IEnumerable<string> fileNames, int minChars = 5, int maxChars = 20)
        {
            if (!fileNames.Any()) return string.Empty;
            
            var sortedNames = fileNames.OrderBy(f => f).ToList();
            string firstFile = sortedNames.First();
            string prefix = firstFile;
            
            foreach (var fileName in sortedNames.Skip(1))
            {
                prefix = new string(prefix.TakeWhile((c, i) => 
                    i < fileName.Length && 
                    i < maxChars && // 不超过最大比对长度
                    fileName[i] == c).ToArray());
                
                if (prefix.Length < minChars) return string.Empty; // 小于最小长度直接返回
            }
            
            // 提取有效前缀 (去除末尾数字、分隔符和扩展名)
            var validPrefix = System.Text.RegularExpressions.Regex.Match(
                Path.GetFileNameWithoutExtension(prefix),
                @"^(.+?)[\d_-]*$").Groups[1].Value;
                
            return validPrefix.Length >= minChars ? validPrefix : string.Empty;
        }

        public IEnumerable<string> GetFilesToCopy(IEnumerable<string> allFiles, IEnumerable<string> filesToMerge)
        {
            return allFiles.Except(filesToMerge);
        }
    }
}
