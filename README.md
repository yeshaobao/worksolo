# WorkSolo

![WorkSolo Logo](Assets/worksolo-mark.png)

WorkSolo 是一个面向个人使用的 Windows 轻量桌面应用，用来记录工作事项、持续推进处理过程，并按时间和项目维度做复盘。

当前仓库对应版本：`v1.1.0`

## 产品定位

WorkSolo 重点解决三件事：

- 把个人工作事项从“记下来”推进到“闭环”
- 把推进过程记录下来，方便后续复盘和写汇报
- 保持本地保存、启动轻量、上手简单

## 当前功能

### 工作台

- 快速新增事项
- 查看 `待推进 / 超期 / 异常阻塞 / 最近更新` 四类状态入口
- 点击状态卡后在右侧展开对应事项明细
- 提供每日一句展示区

### 事项清单

- 新增、编辑、删除事项
- 状态流转：进行中、完成、取消
- 搜索、筛选、快速新增
- 右侧轻量预览，复杂编辑放入独立窗口

### 事项进展记录

- 为事项持续追加进展
- 记录日期、进展内容、问题、下一步
- 支持“需继续跟进”和“关键节点”标记

### 项目分类

- 新建、编辑、删除项目
- 启用和停用项目
- 明确区分“新建项目”和“编辑项目”

### 总结复盘

- 按日 / 周 / 月查看统计结果
- 从统计卡继续下钻到具体事项
- 从项目快照继续查看项目下事项

### 设置与更新

- 主题模式切换
- 默认启动页设置
- 到期提醒开关
- 查看版本信息和更新历史

### 数据保存

- 数据固定保存到本机 `AppData\Local\WorkSolo`
- 兼容旧版数据迁移
- 自动保留最近备份

## v1.1.0 重点改动

- 工作台和总结复盘支持继续下钻到事项
- 事项新增进展记录能力
- 事项编辑改为独立窗口，提升日常使用体验
- 页面按钮、布局和文案做了一轮统一整理
- 数据继续保存在固定本地目录，升级时保留既有数据

详细发布说明见：

- [ReleaseNotes-v1.1.0.md](ReleaseNotes-v1.1.0.md)

## 下载使用

普通使用者建议直接从 GitHub Releases 下载：

- [WorkSolo V1.1.0 Release](https://github.com/yeshaobao/worksolo/releases/tag/v1.1.0)

发布包下载地址：

- [WorkSolo-v1.1.0-windows-x64.zip](https://github.com/yeshaobao/worksolo/releases/download/v1.1.0/WorkSolo-v1.1.0-windows-x64.zip)

## 本地运行

先在本地构建：

```powershell
dotnet build .\WorkClosure.csproj -p:Platform=x64
```

构建完成后可从输出目录启动：

```text
bin\x64\Debug\net8.0-windows10.0.19041.0\WorkSolo.exe
```

如果你本地额外保留了根目录 `WorkSolo.exe` 或 `AppLive\WorkSolo.exe`，那属于本地运行产物，不是源码仓库默认内容。

## 数据说明

- 正式数据目录：`C:\Users\你的用户名\AppData\Local\WorkSolo`
- 当前数据文件：`C:\Users\你的用户名\AppData\Local\WorkSolo\data.json`
- 自动备份目录：`C:\Users\你的用户名\AppData\Local\WorkSolo\backups`

如果旧版本把数据保存在项目目录下的 `data\data.json`，新版本首次启动时会自动尝试迁移。

## 仓库说明

这个 GitHub 仓库默认提交的是源码，不包含完整运行产物：

- 不提交根目录 `WorkSolo.exe`
- 不提交 `AppLive/` 运行目录
- 不提交本地运行数据

如果要给别人直接双击使用，建议通过 GitHub Releases 发布压缩包版本。

## 目录说明

- `Pages/`：页面视图
- `ViewModels/`：页面状态与交互逻辑
- `Models/`：数据模型
- `Services/`：状态管理、存储、统计逻辑
- `Styles/`：主题和样式资源
- `Assets/`：图标与界面资源
- `Dialogs/`：独立编辑窗口
- `使用说明.md`：中文使用手册

## 后续方向

- 汇报素材自动整理
- 标签与备注增强
- 导出复盘结果
- 周期性任务
- 更完整的设置中心

