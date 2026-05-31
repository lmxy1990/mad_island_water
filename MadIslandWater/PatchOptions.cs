namespace MadIslandWater;

internal sealed record PatchOptions(
    string GameDirectory,
    string DlcSourceFile,
    bool InstallDlc,
    bool ApplyMosaicPatch,
    bool BackupFiles,
    long? MosaicShaderPathId);
