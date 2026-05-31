# MadIslandWater

MadIslandWater is a small .NET 10 Windows utility for applying the Mad Island official DLC file and patching the mosaic shader value in `data.unity3d`.

## Features

- Select the Mad Island game directory.
- Copy the official DLC file to `Mad Island_Data\StreamingAssets\DLC\dlc_00`.
- Patch `sharedassets0.assets` inside `data.unity3d`.
- Default mosaic shader PathID is `1964`; leave it empty to scan automatically.
- Apply the legacy decode workaround by copying `UnityPlayer.dll` to `Mad Island_Data\StreamingAssets\XML\none.bat`.
- Create a timestamped backup under `Mad Island_Data\tmp\backup` before patching.

## DLC

The official DLC file is not included in this repository or release assets. Download it from the official site and keep it as-is. Even if the downloaded file is named `dlc_00.zip`, it is a UnityFS file and should not be extracted.

In the app, select the downloaded file manually. The tool copies it to the game as `dlc_00`.

## Docs

- [Mad Island NPC Workshop 中文图文教程](docs/npc-workshop-guide.md)

## Build

```powershell
dotnet build .\MadIslandWater\MadIslandWater.csproj -c Release
```

The project references `third_party\AssetsTools.NET\AssetsTools.NET.dll`, which is included because the local patching code depends on this specific API shape.

## Publish

```powershell
dotnet publish .\MadIslandWater\MadIslandWater.csproj -c Release -r win-x64 --self-contained false -o .\publish\MadIslandWater
```

The published app requires the .NET 10 Windows Desktop Runtime.

## CLI

The executable also has a hidden CLI mode:

```powershell
MadIslandWater.exe --cli --game "D:\Program Files (x86)\Steam\steamapps\common\Mad Island" --dlc "D:\Downloads\dlc_00.zip"
```

Useful options:

- `--pathid 1964`: use a specific shader PathID.
- `--scan`: skip the preferred PathID and scan shaders automatically.
- `--no-dlc`: do not install DLC.
- `--no-mosaic`: do not patch the mosaic shader.
- `--no-legacy`: do not apply the legacy decode workaround.
- `--no-backup`: do not create a `data.unity3d` backup.
