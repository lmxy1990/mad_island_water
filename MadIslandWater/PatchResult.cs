namespace MadIslandWater;

internal sealed record PatchResult(
    bool DlcInstalled,
    bool MosaicPatched,
    IReadOnlyList<string> BackupFiles);
