
# BatchTextProcessor - 文本批量处理工具

## 项目简介
一个基于WPF开发的文本批量处理工具，提供文本合并和拆分功能。

## 主要功能
- 文本文件合并：支持多文件智能合并
- 文本文件拆分：支持按规则拆分大文件
- 文件预览：提供导出前预览功能
- 批量操作：支持批量添加、删除文件

## 环境要求
- .NET 9.0
- Windows 10/11

## 使用说明
1. 下载并解压发布包
2. 运行`BatchTextProcessor.exe`
3. 使用界面：
   - "文本合并"标签：添加文件并设置合并参数
   - "文本拆分"标签：选择文件并设置拆分规则

## 构建方法
```bash
dotnet build
```

## 运行方法
```bash
dotnet run
```

## 项目结构
```
Assets/        # 资源文件
Behaviors/     # WPF行为
Converters/    # 值转换器
Models/        # 数据模型
Services/      # 业务服务
Utils/         # 工具类
ViewModels/    # 视图模型
Views/         # 视图文件
```

## 许可证
MIT License
