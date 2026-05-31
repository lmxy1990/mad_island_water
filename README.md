# MadIslandWater

MadIslandWater 是一个基于 .NET 10 的 Windows 小工具，用来安装 Mad Island 官方 DLC，并把马赛克 Shader 参数改为 0。

## 功能

- 选择 Mad Island 游戏目录。
- 自动安装官方 DLC 到 `Mad Island_Data\StreamingAssets\DLC\dlc_00`。
- 如果游戏仍是 `data.unity3d` bundle，会先解包到同一个 `Mad Island_Data` 目录，再把原 `data.unity3d` 改名为 `data.unity3d.disabled.时间戳`。
- 直接修改解包后的 `sharedassets0.assets`，避免重写 bundle 导致游戏打不开。
- 可一键还原资源：恢复被改名的 `data.unity3d`，并删除解包出来的资源文件。
- 默认马赛克 Shader PathID 为 `1603`；留空可自动扫描。
- 修改前会在 `Mad Island_Data\tmp\backup` 创建带时间戳的备份。

## DLC

官方 DLC 文件放在仓库的 `dlc\dlc_00.zip`，这是当前默认使用的新版 DLC。

旧版 DLC 保留在 `dlc\dlc_00_legacy.zip`，通过 Git LFS 管理，仅作为备用文件。

注意：这个文件虽然叫 `dlc_00.zip`，但它不是普通压缩包，不需要解压，也不要改内容。程序会把它复制到游戏目录，并命名为 `dlc_00`。

程序从仓库目录或带有 `dlc` 子目录的发布目录启动时，会自动找到 `dlc\dlc_00.zip`。如果你想使用其它 DLC 文件，也可以在界面里手动选择。

## 使用

1. 启动 `MadIslandWater.exe`。
2. 选择游戏目录，例如：

```text
D:\Program Files (x86)\Steam\steamapps\common\Mad Island
```

3. 确认 DLC 文件路径，默认会使用仓库里的 `dlc\dlc_00.zip`。
4. PathID 默认使用 `1603`，一般不需要修改；如果游戏版本变化，也可以清空输入框让程序自动扫描。
5. 点击执行，等待完成。

## 还原

点击界面里的“还原资源”会执行以下操作：

- 找到最新的 `Mad Island_Data\data.unity3d.disabled.*`。
- 将它改回 `Mad Island_Data\data.unity3d`。
- 按原 bundle 文件列表删除解包出来的 loose 资源文件。
- 不会删除 `Mad Island_Data\StreamingAssets\DLC\dlc_00`。

## 文档

- [Mad Island NPC Workshop 中文图文教程](docs/npc-workshop-guide.md)

## 构建

使用 Visual Studio 时，直接打开根目录下的 `MadIslandWater.sln`。

命令行构建：

```powershell
.\scripts\build.ps1
```

项目引用了 `third_party\AssetsTools.NET\AssetsTools.NET.dll`。该依赖已放入仓库，因为当前补丁逻辑依赖这个版本的 API。

## 发布

```powershell
.\scripts\publish.ps1
```

默认发布到 `dist`，并会把 `dlc\dlc_00.zip` 一起复制到发布目录。发布后的程序需要安装 .NET 10 Windows Desktop Runtime。

## 命令行模式

程序也提供隐藏的命令行模式：

```powershell
MadIslandWater.exe --cli --game "D:\Program Files (x86)\Steam\steamapps\common\Mad Island" --dlc "D:\Downloads\dlc_00.zip"
```

可用参数：

- `--pathid 1603`：使用指定 Shader PathID。
- `--scan`：跳过默认 PathID，自动扫描 Shader。
- `--no-dlc`：不安装 DLC。
- `--no-mosaic`：不修改马赛克 Shader。
- `--no-backup`：不创建资源文件备份。
- `--restore`：还原被改名的 `data.unity3d`，并删除解包出来的资源文件。
