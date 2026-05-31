using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace MadIslandWater;

internal sealed class MadIslandPatcher
{
    public const long CurrentMosaicShaderPathId = 1964;

    private static readonly byte[] UnityFsHeader = "UnityFS"u8.ToArray();
    private static readonly byte[] MosaicShaderName = "Ist/MosaicField"u8.ToArray();
    private static readonly byte[] BlockSizeName = "_BlockSize"u8.ToArray();
    private static readonly byte[] Float15 = [0x00, 0x00, 0x70, 0x41];
    private static readonly byte[] Float0 = [0x00, 0x00, 0x00, 0x00];

    private readonly Action<string> log;

    public MadIslandPatcher(Action<string> log)
    {
        this.log = log;
    }

    public PatchResult Apply(PatchOptions options)
    {
        ValidateGameDirectory(options.GameDirectory);

        var gameDataDirectory = Path.Combine(options.GameDirectory, "Mad Island_Data");
        var dataBundle = Path.Combine(gameDataDirectory, "data.unity3d");
        var backupFile = options.BackupFiles && options.ApplyMosaicPatch
            ? CreateBackup(dataBundle)
            : null;

        var dlcInstalled = false;
        var mosaicPatched = false;
        var legacyDecodeInstalled = false;

        if (options.InstallDlc)
        {
            InstallDlc(options.GameDirectory, options.DlcSourceFile);
            dlcInstalled = true;
        }

        if (options.ApplyMosaicPatch)
        {
            PatchMosaicShaderInBundle(dataBundle, options.MosaicShaderPathId);
            mosaicPatched = true;
        }

        if (options.ApplyLegacyDecode)
        {
            InstallLegacyDecodeFile(options.GameDirectory);
            legacyDecodeInstalled = true;
        }

        return new PatchResult(dlcInstalled, mosaicPatched, legacyDecodeInstalled, backupFile);
    }

    public void ValidateGameDirectory(string gameDirectory)
    {
        if (string.IsNullOrWhiteSpace(gameDirectory))
        {
            throw new InvalidOperationException("请选择游戏目录。");
        }

        if (!Directory.Exists(gameDirectory))
        {
            throw new DirectoryNotFoundException($"游戏目录不存在：{gameDirectory}");
        }

        var exe = Path.Combine(gameDirectory, "Mad Island.exe");
        var unityPlayer = Path.Combine(gameDirectory, "UnityPlayer.dll");
        var dataBundle = Path.Combine(gameDirectory, "Mad Island_Data", "data.unity3d");
        var xmlDirectory = Path.Combine(gameDirectory, "Mad Island_Data", "StreamingAssets", "XML");

        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("没有找到 Mad Island.exe，请选择 Steam 里的 Mad Island 根目录。", exe);
        }

        if (!File.Exists(unityPlayer))
        {
            throw new FileNotFoundException("没有找到 UnityPlayer.dll。", unityPlayer);
        }

        if (!File.Exists(dataBundle))
        {
            throw new FileNotFoundException("没有找到 Mad Island_Data\\data.unity3d。", dataBundle);
        }

        if (!Directory.Exists(xmlDirectory))
        {
            throw new DirectoryNotFoundException($"没有找到 XML 目录：{xmlDirectory}");
        }
    }

    private void InstallDlc(string gameDirectory, string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
        {
            throw new InvalidOperationException("请选择本地 DLC 文件。");
        }

        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException("DLC 文件不存在。", sourceFile);
        }

        EnsureUnityFsFile(sourceFile, "DLC 文件不是 UnityFS 文件。官网 DLC 虽然可能显示为 .zip，但不应解压，内容应以 UnityFS 开头。");

        var dlcDirectory = Path.Combine(gameDirectory, "Mad Island_Data", "StreamingAssets", "DLC");
        Directory.CreateDirectory(dlcDirectory);

        var destination = Path.Combine(dlcDirectory, "dlc_00");
        log($"复制 DLC：{sourceFile}");
        File.Copy(sourceFile, destination, overwrite: true);
        log($"已安装 DLC 到：{destination}");
    }

    private void InstallLegacyDecodeFile(string gameDirectory)
    {
        var source = Path.Combine(gameDirectory, "UnityPlayer.dll");
        var destinationDirectory = Path.Combine(gameDirectory, "Mad Island_Data", "StreamingAssets", "XML");
        var destination = Path.Combine(destinationDirectory, "none.bat");

        Directory.CreateDirectory(destinationDirectory);
        log("复制 UnityPlayer.dll 到 XML\\none.bat。");
        File.Copy(source, destination, overwrite: true);
        log($"已写入旧解码文件：{destination}");
    }

    private void PatchMosaicShaderInBundle(string bundlePath, long? preferredPathId)
    {
        EnsureUnityFsFile(bundlePath, "data.unity3d 不是 UnityFS bundle。");
        log("读取 data.unity3d。");

        var temporaryFile = Path.Combine(Path.GetDirectoryName(bundlePath)!, "tmp", $"data.unity3d.{DateTime.Now:yyyyMMddHHmmss}.tmp");
        Directory.CreateDirectory(Path.GetDirectoryName(temporaryFile)!);

        using var inputStream = File.Open(bundlePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var manager = new AssetsManager();
        var bundle = manager.LoadBundleFile(inputStream, bundlePath, unpackIfPacked: true);
        var assetsFile = manager.LoadAssetsFileFromBundle(bundle, "sharedassets0.assets", loadDeps: false);

        var target = FindMosaicShader(assetsFile, preferredPathId);
        var originalData = ReadAssetBytes(assetsFile, target);
        var patchedData = PatchBlockSize(originalData);

        if (originalData.AsSpan().SequenceEqual(patchedData))
        {
            log("马赛克参数已经是 0，跳过 bundle 写入。");
            return;
        }

        var assetReplacers = new List<AssetsReplacer>
        {
            new AssetsReplacerFromMemory(
                target.PathId,
                target.TypeId,
                target.ScriptTypeIndex,
                patchedData)
        };

        var bundleReplacers = new List<BundleReplacer>
        {
            new BundleReplacerFromAssets(
                "sharedassets0.assets",
                "sharedassets0.assets",
                assetsFile.file,
                assetReplacers,
                bundle.file.GetFileIndex("sharedassets0.assets"),
                typeMeta: null)
        };

        log("写入补丁后的 data.unity3d 到临时目录。");
        using (var outputStream = File.Open(temporaryFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        using (var writer = new AssetsFileWriter(outputStream))
        {
            bundle.file.Write(writer, bundleReplacers, typeMeta: null);
        }

        manager.UnloadAll(unloadClassData: true);
        inputStream.Close();

        log("替换原 data.unity3d。");
        File.Copy(temporaryFile, bundlePath, overwrite: true);
        File.Delete(temporaryFile);
        log("马赛克 Shader 参数已修改为 0。");
    }

    private AssetFileInfo FindMosaicShader(AssetsFileInstance assetsFile, long? preferredPathId)
    {
        var shaderType = (int)AssetClassID.Shader;
        var reader = assetsFile.file.Reader;

        if (preferredPathId is not null)
        {
            var preferredInfo = assetsFile.file.GetAssetInfo(preferredPathId.Value);
            if (preferredInfo is not null)
            {
                var preferredData = ReadAssetBytes(assetsFile, preferredInfo);
                if (preferredInfo.GetTypeId(assetsFile.file) == shaderType && LooksLikePreferredMosaicShader(preferredInfo, preferredData))
                {
                    log($"按 PathID 定位到 Shader：PathId={preferredInfo.PathId}，Size={preferredInfo.ByteSize}。");
                    reader.Position = 0;
                    return preferredInfo;
                }

                log($"PathID {preferredPathId.Value} 存在，但不是目标 Shader，开始自动扫描。");
            }
            else
            {
                log($"没有找到 PathID {preferredPathId.Value}，开始自动扫描。");
            }
        }

        var candidates = assetsFile.file.GetAssetsOfType(shaderType);

        foreach (var info in candidates)
        {
            var data = ReadAssetBytes(assetsFile, info);
            if (LooksLikeMosaicShader(data))
            {
                log($"自动扫描定位到 Shader：PathId={info.PathId}，Size={info.ByteSize}。");
                reader.Position = 0;
                return info;
            }
        }

        throw new InvalidOperationException("没有在 sharedassets0.assets 中找到 Ist/MosaicField Shader。游戏版本可能已经变化。");
    }

    private static bool LooksLikeMosaicShader(byte[] data)
    {
        return IndexOf(data, MosaicShaderName) >= 0 && IndexOf(data, BlockSizeName) >= 0;
    }

    private static bool LooksLikePreferredMosaicShader(AssetFileInfo info, byte[] data)
    {
        return LooksLikeMosaicShader(data)
            || (info.ByteSize is >= 5000 and <= 9000
                && IndexOf(data, BlockSizeName) >= 0
                && HasPatchableBlockSize(data));
    }

    private static bool HasPatchableBlockSize(byte[] data)
    {
        var blockNameOffset = IndexOf(data, BlockSizeName);
        if (blockNameOffset < 0)
        {
            return false;
        }

        var searchStart = blockNameOffset + BlockSizeName.Length;
        var searchLength = Math.Min(128, data.Length - searchStart);
        if (searchLength <= 0)
        {
            return false;
        }

        var searchArea = data.AsSpan(searchStart, searchLength);
        return IndexOf(searchArea, Float15) >= 0 || HasLikelyPatchedBlockSize(searchArea);
    }

    private static byte[] ReadAssetBytes(AssetsFileInstance assetsFile, AssetFileInfo info)
    {
        if (info.ByteSize > int.MaxValue)
        {
            throw new InvalidOperationException($"资产过大，无法读取：PathId={info.PathId}, Size={info.ByteSize}");
        }

        var reader = assetsFile.file.Reader;
        reader.Position = info.GetAbsoluteByteStart(assetsFile.file);
        return reader.ReadBytes((int)info.ByteSize);
    }

    private byte[] PatchBlockSize(byte[] data)
    {
        var blockNameOffset = IndexOf(data, BlockSizeName);
        if (blockNameOffset < 0)
        {
            throw new InvalidOperationException("Shader 中没有找到 _BlockSize。");
        }

        var searchStart = blockNameOffset + BlockSizeName.Length;
        var searchLength = Math.Min(128, data.Length - searchStart);
        var searchArea = data.AsSpan(searchStart, searchLength);
        var valueOffset = IndexOf(searchArea, Float15);

        if (valueOffset < 0)
        {
            if (HasLikelyPatchedBlockSize(searchArea))
            {
                log("_BlockSize 默认值已经是 0.0。");
                return data;
            }

            throw new InvalidOperationException("找到 _BlockSize，但没有在附近找到默认值 15.0。");
        }

        var patched = data.ToArray();
        Float0.CopyTo(patched, searchStart + valueOffset);
        log("已将 _BlockSize 默认值 15.0 修改为 0.0。");
        return patched;
    }

    private static bool HasLikelyPatchedBlockSize(ReadOnlySpan<byte> searchArea)
    {
        var blockSizeHeader = new byte[]
        {
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        var headerOffset = IndexOf(searchArea, blockSizeHeader);
        if (headerOffset < 0)
        {
            return false;
        }

        var valueStart = headerOffset + blockSizeHeader.Length;
        return valueStart + Float0.Length <= searchArea.Length
            && searchArea.Slice(valueStart, Float0.Length).SequenceEqual(Float0);
    }

    private string CreateBackup(string filePath)
    {
        var backupDirectory = Path.Combine(Path.GetDirectoryName(filePath)!, "tmp", "backup");
        Directory.CreateDirectory(backupDirectory);

        var backup = Path.Combine(backupDirectory, $"{Path.GetFileName(filePath)}.{DateTime.Now:yyyyMMddHHmmss}.bak");
        log($"备份 data.unity3d：{backup}");
        File.Copy(filePath, backup, overwrite: false);
        return backup;
    }

    private static void EnsureUnityFsFile(string filePath, string errorMessage)
    {
        Span<byte> header = stackalloc byte[UnityFsHeader.Length];
        using var stream = File.OpenRead(filePath);

        if (stream.Read(header) != UnityFsHeader.Length || !header.SequenceEqual(UnityFsHeader))
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        return IndexOf(haystack.AsSpan(), needle);
    }

    private static int IndexOf(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle)
    {
        if (needle.IsEmpty)
        {
            return 0;
        }

        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            if (haystack.Slice(i, needle.Length).SequenceEqual(needle))
            {
                return i;
            }
        }

        return -1;
    }
}
