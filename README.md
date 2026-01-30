# CFramework Systems

基于 CFramework.Core 的系统拓展包，为 Unity 游戏开发提供了一套完整的系统模块，涵盖资产管理、音频管理、存档管理、UI 管理、定时器、场景管理和 Unity 对象容器等功能。

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![zread](https://img.shields.io/badge/Ask_Zread-_.svg?style=flat&color=00b0aa&labelColor=000000&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTQuOTYxNTYgMS42MDAxSDIuMjQxNTZDMS44ODgxIDEuNjAwMSAxLjYwMTU2IDEuODg2NjQgMS42MDE1NiAyLjI0MDFWNC45NjAxQzEuNjAxNTYgNS4zMTM1NiAxLjg4ODEgNS42MDAxIDIuMjQxNTYgNS42MDAxSDQuOTYxNTZDNS4zMTUwMiA1LjYwMDEgNS42MDE1NiA1LjMxMzU2IDUuNjAxNTYgNC45NjAxVjIuMjQwMUM1LjYwMTU2IDEuODg2NjQgNS4zMTUwMiAxLjYwMDEgNC45NjE1NiAxLjYwMDFaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik00Ljk2MTU2IDEwLjM5OTlIMi4yNDE1NkMxLjg4ODEgMTAuMzk5OSAxLjYwMTU2IDEwLjY4NjQgMS42MDE1NiAxMS4wMzk5VjEzLjc1OTlDMS42MDE1NiAxNC4xMTM0IDEuODg4MSAxNC4zOTk5IDIuMjQxNTYgMTQuMzk5OUg0Ljk2MTU2QzUuMzE1MDIgMTQuMzk5OSA1LjYwMTU2IDE0LjExMzQgNS42MDE1NiAxMy43NTk5VjExLjAzOTlDNS42MDE1NiAxMC42ODY0IDUuMzE1MDIgMTAuMzk5OSA0Ljk2MTU2IDEwLjM5OTlaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik0xMy43NTg0IDEuNjAwMUgxMS4wMzg0QzEwLjY4NSAxLjYwMDEgMTAuMzk4NCAxLjg4NjY0IDEwLjM5ODQgMi4yNDAxVjQuOTYwMUMxMC4zOTg0IDUuMzEzNTYgMTAuNjg1IDUuNjAwMSAxMS4wMzg0IDUuNjAwMUgxMy43NTg0QzE0LjExMTkgNS42MDAxIDE0LjM5ODQgNS4zMTM1NiAxNC4zOTg0IDQuOTYwMVYyLjI0MDFDMTQuMzk4NCAxLjg4NjY0IDE0LjExMTkgMS42MDAxIDEzLjc1ODQgMS42MDAxWiIgZmlsbD0iI2ZmZiIvPgo8cGF0aCBkPSJNNCAxMkwxMiA0TDQgMTJaIiBmaWxsPSIjZmZmIi8%2BCjxwYXRoIGQ9Ik00IDEyTDEyIDQiIHN0cm9rZT0iI2ZmZiIgc3Ryb2tlLXdpZHRoPSIxLjUiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K&logoColor=ffffff)](https://zread.ai/cnoom/CFramewrok-System)
## 特性

### 核心系统模块

- **📦 AssetsSystem** - 基于 Unity Addressables 的资源管理系统
  - 单资源和批量资源加载
  - 引用计数管理和自动释放
  - 资源加载进度跟踪
  - 灵活的资源接收器模式

- **🎵 AudioSystem** - 完整的音频管理系统
  - 背景音乐淡入淡出和 Crossfade
  - 音效对象池管理
  - Master/Music/SFX 三级音量控制
  - 音频设置自动持久化
  - 全局暂停支持

- **💾 SaveSystem** - 可靠的存档管理系统
  - 多档案（Profile）多槽位（Slot）支持
  - 脏数据检测和自动保存
  - 原子写入保证数据安全
  - JSON 序列化

- **🎨 UISystem** - 强大的 UI 管理系统
  - 多层级 UI 管理
  - UI 对象池优化
  - 灵活的过渡动画
  - 完整的生命周期管理
  - 错误处理和重试机制

- **⏱️ TimerSystem** - 高效的定时器系统
  - 单次和循环定时器
  - 标签分组管理
  - 暂停/恢复控制
  - 支持缩放和非缩放时间

- **🎬 SceneSystem** - 场景管理系统
  - 场景加载/卸载
  - 场景切换和进度跟踪

- **🔗 UnityContainerSystem** - Unity 对象容器系统
  - 对象自动绑定
  - 作用域隔离
  - 组件快速查询
  - 并发安全

### 编辑器工具

- **Addressable 工具**
  - 代码生成和配置管理
  - 右键菜单快捷操作
  - 资源文件夹自动同步

- **系统配置编辑器**
  - AudioConfig 可视化配置
  - UIConfig 界面配置
  - SaveSystem 存档管理

### 工具类库

- **Extensions** - 丰富的扩展方法
  - Color、GameObject、Transform、Vector 等
  - 状态机扩展
  - 数字转换扩展

- **Utilities** - 实用工具类
  - FSM（有限状态机）
  - Blackboard（黑板数据存储）
  - EasyEvent（轻量级事件）
  - GuideMask（新手引导遮罩）

## 安装

### 通过 Package Manager 安装

1. 在 Unity 项目的 `Packages/manifest.json` 文件中添加以下依赖：

```json
{
  "dependencies": {
    "com.cnoom.cframework": "https://github.com/cnoom/CFramework.Systems.git#upm",
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    "com.cysharp.unitask": "2.5.1",
    "com.unity.addressables": "1.22.3"
  }
}
```

2. 确保已安装 CFramework.Core 作为基础依赖

3. 重新打开项目或点击 Package Manager 中的更新按钮

### 手动安装

1. 下载本仓库
2. 将 `Assets/com.cnoom.cframework.systems` 文件夹复制到您的 Unity 项目中
3. 确保项目包含所需的依赖项

## 快速开始

### 初始化框架

1. 在指定的场景新建一个 GameObject
2. 将 `CFrameworkUnityEntry` 组件添加到该 GameObject 上
3. 将 `Assets/CFramework/Config/CFrameworkConfig.asset` 拖拽到 `CFrameworkUnityEntry` 所需位置即可

框架会自动扫描并注册所有带有 `[AutoModule]` 特性的模块，无需手动注册。

### 使用示例

#### 播放音频

```csharp
using CFramework.Core;
using CFramework.Systems.AudioSystem;

// 播放背景音乐
CF.Execute(new AudioCommands.PlayMusic(
    clipKey: "BGM_MainMenu",
    fadeInSeconds: 1.5f,
    fadeOutInSeconds: 0.5f,
    loop: true
)).Forget();

// 播放音效
CF.Execute(new AudioCommands.PlaySfx(
    clipKey: "SFX_Click",
    volume: 0.8f,
    pitch: 1.0f
)).Forget();

// 设置音量
CF.Execute(new AudioCommands.SetVolume(
    category: AudioCategory.Music,
    volume: 0.7f
)).Forget();
```

#### 存档管理

```csharp
using CFramework.Core;
using CFramework.Systems.SaveSystem;
using CFramework.Systems.SaveSystem.Data;

// 获取当前存档槽位
Slot saveSlot = await CF.Query<SaveQueries.CurrentProfileSlot, Slot>(
    new SaveQueries.CurrentProfileSlot("player_data")
);

// 保存数据
saveSlot.SetInt("level", 5);
saveSlot.SetFloat("health", 100.0f);
saveSlot.SetString("player_name", "Hero");

// 提交保存
CF.Execute(new SaveCommands.SaveSlot("player_data")).Forget();

// 读取数据
int level = saveSlot.GetInt("level", 1);
float health = saveSlot.GetFloat("health", 0.0f);
string playerName = saveSlot.GetString("player_name", "");
```

#### UI 管理

```csharp
using CFramework.Core;
using CFramework.Systems.UISystem;

// 打开 UI
CF.Execute(new UICommands.OpenView(
    key: "MainMenuPanel",
    layer: "Normal",
    transitionName: "Fade",
    seconds: 0.3f
)).Forget();

// 关闭 UI
CF.Execute(new UICommands.CloseTop(
    layer: "Normal",
    seconds: 0.3f
)).Forget();

// 查询当前打开的 UI
ViewInfo[] openViews = await CF.Query<UIQueries.GetOpenViews, ViewInfo[]>(
    new UIQueries.GetOpenViews("Normal")
);
```

#### 定时器使用

```csharp
using CFramework.Core;
using CFramework.Systems.TimerSystem;

// 启动定时器
CF.Execute(new TimerCommands.StartTimer(
    id: "cooldown_timer",
    durationSeconds: 5.0f,
    loop: false,
    tag: "cooldown"
)).Forget();

// 监听定时器完成事件
CF.Listen<TimerBroadcasts.TimerCompleted>(evt =>
{
    Debug.Log($"Timer {evt.Id} completed!");
});

// 暂停/恢复定时器
CF.Execute(new TimerCommands.PauseTimer("cooldown_timer")).Forget();
CF.Execute(new TimerCommands.ResumeTimer("cooldown_timer")).Forget();
```

#### 加载资源

```csharp
using CFramework.Core;
using CFramework.Systems.AssetsSystem;

// 加载单个资源
GameObject prefab = await CF.Query<AssetsQueries.Asset, GameObject>(
    new AssetsQueries.Asset("PlayerPrefab")
);

// 加载批量资源
AudioClip[] audioClips = await CF.Query<AssetsQueries.Assets, AudioClip[]>(
    new AssetsQueries.Assets("AllSFX")
);

// 释放资源
CF.Execute(new AssetsCommands.ReleaseAsset<GameObject>("PlayerPrefab")).Forget();
```

## 系统详解

### AssetsSystem

资源管理系统基于 Unity Addressables，提供了更加便捷的加载和管理接口。

**核心功能：**
- 异步资源加载
- 引用计数自动管理
- 进度跟踪和回调
- 自定义资源接收器

**配置：**
确保 Addressables 组已正确配置，并设置合适的标签。

### AudioSystem

音频系统提供了完整的背景音乐和音效管理功能。

**核心功能：**
- 双音源 Crossfade 实现
- 音效对象池优化性能
- 音量分级控制
- 设置自动持久化

**配置：**
创建 `AudioConfig` ScriptableObject 并配置默认音量、池大小等参数。

### SaveSystem

存档系统使用 JSON 序列化，支持多档案多槽位管理。

**核心功能：**
- 多档案隔离
- 原子写入保证安全
- 脏数据自动保存
- 类型安全的数据访问

**数据存储路径：**
```
{Application.persistentDataPath}/cframework/saves/{profileId}/{slotId}.json
```

### UISystem

UI 系统提供了完整的视图管理功能。

**核心功能：**
- 多层级管理
- 对象池优化
- 过渡动画支持
- 生命周期钩子
- 错误重试机制

**视图生命周期：**
```
Create → ShowBefore → Show → Hide → Close
```

**配置：**
创建 `UIConfig` ScriptableObject 并配置层级、过渡效果等参数。

### TimerSystem

定时器系统提供了高效的定时任务管理。

**核心功能：**
- 单次/循环触发
- 标签分组
- 暂停/恢复
- 缩放/非缩放时间

### UnityContainerSystem

Unity 对象容器系统提供了对象自动绑定和查询功能。

**核心功能：**
- 自动绑定 GameObject 和组件
- 作用域隔离
- 并发安全
- 组件快速查询

## 编辑器工具

### Addressable 工具

提供了以下快捷操作：
- 右键菜单生成 Addressable 代码
- 文件夹自动同步 Addressable 配置
- 可视化管理窗口

访问路径：`Assets > Create > CFramework > Addressable`

### UI 配置工具

可视化配置 UI 层级和过渡效果。

访问路径：`CFramework > UI Config`

## 依赖项

- **CFramework.Core** - 基础框架（必须）
- **Unity 2021.3+** - Unity 版本要求
- **com.unity.addressables** (1.22.3) - Addressable Asset System
- **com.cysharp.unitask** (2.5.1) - 异步任务库
- **com.unity.nuget.newtonsoft-json** (3.2.1) - JSON 序列化

## 架构设计

### 模块化设计

每个系统都是独立的模块，遵循以下原则：
- **单一职责** - 每个系统专注于特定功能
- **松耦合** - 系统间通过 Command/Query/Broadcast 通信
- **可扩展** - 支持自定义扩展和配置

### 通信机制

```csharp
// Command - 执行命令
CF.Execute(new SomeCommand()).Forget();

// Query - 查询数据
var result = await CF.Query<SomeQuery, TResult>(new SomeQuery());

// Broadcast - 广播事件
CF.Broadcast(new SomeEvent()).Forget();
```

## 贡献指南

欢迎提交 Issue 和 Pull Request！

在提交 PR 之前，请确保：
1. 代码符合项目编码规范
2. 添加必要的单元测试
3. 更新相关文档
4. 通过所有 CI 检查

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 作者

**Cnoom** - [GitHub](https://github.com/cnoom) - cnoom@qq.com

## 致谢

感谢所有为 CFramework 做出贡献的开发者！

## 相关链接

- [CFramework.Core](https://github.com/cnoom/CFramewrok-Core) - 基础框架
- [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest) - Addressable 资源系统文档
- [UniTask](https://github.com/Cysharp/UniTask) - 异步任务库

---

**Made with ❤️ by Cnoom**
