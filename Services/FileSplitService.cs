
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatchTextProcessor.Services
{
    public class FileSplitService
    {
        public void SplitTextFileByLines(string filePath, int linesPerFile, ILogger logger)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
                
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid file path");

            var outputDir = Path.Combine(directory, fileName);
            Directory.CreateDirectory(outputDir);

            var lines = File.ReadLines(filePath);
            int fileCount = 1;
            var currentLines = new List<string>();

            foreach (var line in lines)
            {
                currentLines.Add(line);
                if (currentLines.Count >= linesPerFile)
                {
                    WriteSplitFile(outputDir, Path.GetFileName(filePath), fileCount++, currentLines, logger);
                    currentLines.Clear();
                }
            }

            if (currentLines.Any())
            {
                WriteSplitFile(outputDir, Path.GetFileName(filePath), fileCount, currentLines, logger);
            }

            logger.Log($"文件拆分完成(按行): {filePath} → {fileCount}个文件");
        }

        public void SplitTextFileBySize(string filePath, long sizePerFileBytes, ILogger logger)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
                
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid file path");

            var outputDir = Path.Combine(directory, fileName);
            Directory.CreateDirectory(outputDir);

            using var reader = new StreamReader(filePath);
            int fileCount = 1;
            var currentSize = 0L;
            var currentLines = new List<string>();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                currentLines.Add(line);
                currentSize += System.Text.Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;

                if (currentSize >= sizePerFileBytes)
                {
                    WriteSplitFile(outputDir, Path.GetFileName(filePath), fileCount++, currentLines, logger);
                    currentLines.Clear();
                    currentSize = 0;
                }
            }

            if (currentLines.Any())
            {
                WriteSplitFile(outputDir, Path.GetFileName(filePath), fileCount, currentLines, logger);
            }

            logger.Log($"文件拆分完成(按大小): {filePath} → {fileCount}个文件");
        }

        private void WriteSplitFile(string outputDir, string originalName, int fileNumber, 
                                  List<string> lines, ILogger logger)
        {
            try
            {
                var newFileName = $"{Path.GetFileNameWithoutExtension(originalName)}_{fileNumber}{Path.GetExtension(originalName)}";
                var outputPath = Path.Combine(outputDir, newFileName);
                File.WriteAllLines(outputPath, lines);
                logger.Log($"已创建拆分文件: {outputPath}");
            }
            catch (Exception ex)
            {
                logger.LogError($"写入拆分文件时出错: {ex.Message}");
                throw;
            }
        }
    }
}
