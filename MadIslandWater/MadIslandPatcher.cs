using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace MadIslandWater;

internal sealed class MadIslandPatcher
{
    public const long CurrentMosaicShaderPathId = 1603;

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
        var backupFiles = new List<string>();

        var dlcInstalled = false;
        var mosaicPatched = false;

        if (options.InstallDlc)
        {
            InstallDlc(options.GameDirectory, options.DlcSourceFile);
            dlcInstalled = true;
        }

        if (options.ApplyMosaicPatch)
        {
            PatchMosaicShader(gameDataDirectory, options.MosaicShaderPathId, options.BackupFiles, backupFiles);
            mosaicPatched = true;
        }

        return new PatchResult(dlcInstalled, mosaicPatched, backupFiles);
    }

    public void Restore(string gameDirectory)
    {
        ValidateGameRoot(gameDirectory);

        var gameDataDirectory = Path.Combine(gameDirectory, "Mad Island_Data");
        if (!Directory.Exists(gameDataDirectory))
        {
            throw new DirectoryNotFoundException($"没有找到 Mad Island_Data 目录：{gameDataDirectory}");
        }

        var dataBundlePath = Path.Combine(gameDataDirectory, "data.unity3d");
        if (File.Exists(dataBundlePath))
        {
            EnsureUnityFsFile(dataBundlePath, "data.unity3d 不是 UnityFS bundle。");
            log("data.unity3d 已存在，执行解包资源清理。");
            var currentBundleEntries = ReadBundleEntryPaths(dataBundlePath, gameDataDirectory);
            var cleanupResult = RemoveExtractedFiles(gameDataDirectory, currentBundleEntries);
            LogRestoreCleanupResult(cleanupResult.RemovedFiles, cleanupResult.FailedFiles);
            DeleteDirectoryIfEmpty(Path.Combine(gameDataDirectory, "tmp"));
            return;
        }

        var disabledBundle = FindLatestDisabledBundle(gameDataDirectory);
        EnsureUnityFsFile(disabledBundle.FullName, "被改名的 data.unity3d 不是 UnityFS bundle。");
        log($"找到可还原文件：{disabledBundle.FullName}");

        var extractedPaths = ReadBundleEntryPaths(disabledBundle.FullName, gameDataDirectory);
        File.Move(disabledBundle.FullName, dataBundlePath);
        log($"已恢复 data.unity3d：{dataBundlePath}");

        var (removedFiles, failedFiles) = RemoveExtractedFiles(gameDataDirectory, extractedPaths);
        LogRestoreCleanupResult(removedFiles, failedFiles);
        DeleteDirectoryIfEmpty(Path.Combine(gameDataDirectory, "tmp"));
    }

    public void ValidateGameDirectory(string gameDirectory)
    {
        ValidateGameRoot(gameDirectory);

        var dataBundle = Path.Combine(gameDirectory, "Mad Island_Data", "data.unity3d");
        var sharedAssets = Path.Combine(gameDirectory, "Mad Island_Data", "sharedassets0.assets");

        if (!File.Exists(dataBundle) && !File.Exists(sharedAssets))
        {
            throw new FileNotFoundException("没有找到 Mad Island_Data\\data.unity3d 或 sharedassets0.assets。", dataBundle);
        }

    }

    private static void ValidateGameRoot(string gameDirectory)
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

        if (!File.Exists(exe))
        {
            throw new FileNotFoundException("没有找到 Mad Island.exe，请选择 Steam 里的 Mad Island 根目录。", exe);
        }

        if (!File.Exists(unityPlayer))
        {
            throw new FileNotFoundException("没有找到 UnityPlayer.dll。", unityPlayer);
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

    private void PatchMosaicShader(string gameDataDirectory, long? preferredPathId, bool backupFiles, List<string> backups)
    {
        var sharedAssetsPath = EnsureSharedAssetsFile(gameDataDirectory, backupFiles, backups);
        PatchMosaicShaderInAssetsFile(sharedAssetsPath, preferredPathId, backupFiles, backups);
    }

    private string EnsureSharedAssetsFile(string gameDataDirectory, bool backupFiles, List<string> backups)
    {
        var sharedAssetsPath = Path.Combine(gameDataDirectory, "sharedassets0.assets");
        var bundlePath = Path.Combine(gameDataDirectory, "data.unity3d");

        if (File.Exists(bundlePath))
        {
            EnsureUnityFsFile(bundlePath, "data.unity3d 不是 UnityFS bundle。");
            if (backupFiles)
            {
                backups.Add(CreateBackup(bundlePath));
            }

            ExtractBundleToGameData(bundlePath, gameDataDirectory);

            if (!File.Exists(sharedAssetsPath))
            {
                throw new InvalidOperationException("data.unity3d 已解包，但没有得到 sharedassets0.assets。");
            }

            var disabledBundlePath = RenameBundleOutOfTheWay(bundlePath);
            backups.Add(disabledBundlePath);
            log($"已重命名原 data.unity3d：{disabledBundlePath}");
            log("游戏将使用同目录下解包后的资源文件。");
            return sharedAssetsPath;
        }

        if (File.Exists(sharedAssetsPath))
        {
            log("检测到 sharedassets0.assets，直接修改解包后的资源文件。");
            return sharedAssetsPath;
        }

        throw new FileNotFoundException("没有找到 Mad Island_Data\\data.unity3d 或 sharedassets0.assets。", bundlePath);
    }

    private void ExtractBundleToGameData(string bundlePath, string gameDataDirectory)
    {
        var extractDirectory = Path.Combine(gameDataDirectory, "tmp", $"extract-{DateTime.Now:yyyyMMddHHmmss}");
        Directory.CreateDirectory(extractDirectory);

        log("读取并解包 data.unity3d。");
        var manager = new AssetsManager();
        using var inputStream = File.Open(bundlePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bundle = manager.LoadBundleFile(inputStream, bundlePath, unpackIfPacked: true);

        try
        {
            var extractedCount = 0;
            foreach (var entry in bundle.file.BlockAndDirInfo.DirectoryInfos)
            {
                if ((entry.Flags & 0x02) != 0)
                {
                    continue;
                }

                var destination = GetSafeChildPath(extractDirectory, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                using var output = File.Open(destination, FileMode.Create, FileAccess.Write, FileShare.None);
                var reader = bundle.file.DataReader;
                reader.Position = entry.Offset;
                CopyExactly(reader.BaseStream, output, entry.DecompressedSize);
                extractedCount++;
            }

            log($"已解包 {extractedCount} 个资源文件到临时目录。");
        }
        finally
        {
            manager.UnloadAll(unloadClassData: true);
        }

        var copiedCount = 0;
        foreach (var source in Directory.EnumerateFiles(extractDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(extractDirectory, source);
            var destination = GetSafeChildPath(gameDataDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, overwrite: true);
            copiedCount++;
        }

        Directory.Delete(extractDirectory, recursive: true);
        DeleteDirectoryIfEmpty(Path.GetDirectoryName(extractDirectory)!);
        log($"已把 {copiedCount} 个资源文件放入 Mad Island_Data。");
    }

    private static string RenameBundleOutOfTheWay(string bundlePath)
    {
        var directory = Path.GetDirectoryName(bundlePath)!;
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        for (var suffix = 0; suffix < 100; suffix++)
        {
            var suffixText = suffix == 0 ? string.Empty : $".{suffix}";
            var destination = Path.Combine(directory, $"data.unity3d.disabled.{timestamp}{suffixText}");
            if (!File.Exists(destination))
            {
                File.Move(bundlePath, destination);
                return destination;
            }
        }

        throw new IOException("无法为 data.unity3d 生成可用的改名路径。");
    }

    private static FileInfo FindLatestDisabledBundle(string gameDataDirectory)
    {
        var directory = new DirectoryInfo(gameDataDirectory);
        var candidates = directory
            .EnumerateFiles("data.unity3d.disabled.*", SearchOption.TopDirectoryOnly)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ThenByDescending(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new FileNotFoundException("没有找到 data.unity3d.disabled.*，无法还原。");
        }

        return candidates[0];
    }

    private static List<string> ReadBundleEntryPaths(string bundlePath, string gameDataDirectory)
    {
        var paths = new List<string>();
        var manager = new AssetsManager();
        using var inputStream = File.Open(bundlePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bundle = manager.LoadBundleFile(inputStream, bundlePath, unpackIfPacked: true);

        try
        {
            foreach (var entry in bundle.file.BlockAndDirInfo.DirectoryInfos)
            {
                if ((entry.Flags & 0x02) != 0)
                {
                    continue;
                }

                paths.Add(GetSafeChildPath(gameDataDirectory, entry.Name));
            }
        }
        finally
        {
            manager.UnloadAll(unloadClassData: true);
        }

        return paths;
    }

    private (int RemovedFiles, int FailedFiles) RemoveExtractedFiles(string gameDataDirectory, IEnumerable<string> extractedPaths)
    {
        var removedFiles = 0;
        var failedFiles = 0;
        var parentDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in extractedPaths.OrderByDescending(path => path.Length))
        {
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                parentDirectories.Add(Path.GetDirectoryName(path)!);
                File.Delete(path);
                removedFiles++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                failedFiles++;
                log($"删除失败：{path}；{ex.Message}");
            }
        }

        foreach (var directory in parentDirectories.OrderByDescending(directory => directory.Length))
        {
            DeleteEmptyDirectoriesUpTo(directory, gameDataDirectory);
        }

        return (removedFiles, failedFiles);
    }

    private void LogRestoreCleanupResult(int removedFiles, int failedFiles)
    {
        log($"已删除解包资源文件：{removedFiles} 个。");

        if (failedFiles > 0)
        {
            log($"有 {failedFiles} 个解包资源文件删除失败，请关闭游戏后再次执行还原清理。");
        }
    }

    private void PatchMosaicShaderInAssetsFile(string assetsPath, long? preferredPathId, bool backupFiles, List<string> backups)
    {
        log("读取 sharedassets0.assets。");

        var manager = new AssetsManager();
        var assetsFile = manager.LoadAssetsFile(assetsPath, loadDeps: false);
        var temporaryFile = Path.Combine(Path.GetDirectoryName(assetsPath)!, "tmp", $"sharedassets0.assets.{DateTime.Now:yyyyMMddHHmmss}.tmp");
        Directory.CreateDirectory(Path.GetDirectoryName(temporaryFile)!);

        try
        {
            var target = FindMosaicShader(assetsFile, preferredPathId);
            var originalData = ReadAssetBytes(assetsFile, target);
            var patchedData = PatchBlockSize(originalData);

            if (originalData.AsSpan().SequenceEqual(patchedData))
            {
                log("马赛克参数已经是 0，跳过写入。");
                return;
            }

            if (backupFiles)
            {
                backups.Add(CreateBackup(assetsPath));
            }

            var assetReplacers = new List<AssetsReplacer>
            {
                new AssetsReplacerFromMemory(
                    target.PathId,
                    target.GetTypeId(assetsFile.file),
                    assetsFile.file.GetScriptIndex(target),
                    patchedData)
            };

            log("写入补丁后的 sharedassets0.assets 到临时目录。");
            using (var outputStream = File.Open(temporaryFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var writer = new AssetsFileWriter(outputStream))
            {
                assetsFile.file.Write(writer, 0, assetReplacers, typeMeta: null);
            }
        }
        finally
        {
            manager.UnloadAll(unloadClassData: true);
        }

        log("替换原 sharedassets0.assets。");
        File.Copy(temporaryFile, assetsPath, overwrite: true);
        File.Delete(temporaryFile);
        DeleteDirectoryIfEmpty(Path.GetDirectoryName(temporaryFile)!);
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
            if (info.PathId <= 0)
            {
                continue;
            }

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

    private static string GetSafeChildPath(string baseDirectory, string relativePath)
    {
        var root = Path.GetFullPath(baseDirectory);
        var combined = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootWithSeparator = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(combined, root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"资源路径越界：{relativePath}");
        }

        return combined;
    }

    private static void CopyExactly(Stream source, Stream destination, long length)
    {
        if (length < 0)
        {
            throw new InvalidOperationException("资源文件大小无效。");
        }

        var buffer = new byte[81920];
        var remaining = length;
        while (remaining > 0)
        {
            var readLength = (int)Math.Min(buffer.Length, remaining);
            var read = source.Read(buffer, 0, readLength);
            if (read == 0)
            {
                throw new EndOfStreamException("读取 bundle 资源时提前到达文件末尾。");
            }

            destination.Write(buffer, 0, read);
            remaining -= read;
        }
    }

    private static void DeleteDirectoryIfEmpty(string directory)
    {
        if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    private static void DeleteEmptyDirectoriesUpTo(string directory, string stopDirectory)
    {
        var stop = Path.GetFullPath(stopDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var current = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        while (!string.Equals(current, stop, StringComparison.OrdinalIgnoreCase) && Directory.Exists(current))
        {
            if (Directory.EnumerateFileSystemEntries(current).Any())
            {
                break;
            }

            Directory.Delete(current);
            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
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
        log($"备份 {Path.GetFileName(filePath)}：{backup}");
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
