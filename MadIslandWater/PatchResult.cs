namespace MadIslandWater;

internal sealed record PatchResult(
    bool DlcInstalled,
    bool MosaicPatched,
    bool LegacyDecodeInstalled,
    string? BackupFile);

