using TomodachiCanvasExport;

namespace TomodachiWeb;

/// <summary>
/// Adapts TextureProcessor (file-path API) to work with byte arrays via the
/// WASM virtual filesystem. No modifications to TextureProcessor needed.
/// </summary>
public static class TextureProcessorAdapter
{
    /// <summary>PNG bytes → .canvas.zs / .ugctex.zs / _Thumb_ugctex.zs</summary>
    public static Task<List<OutputFile>> ImportPngAsync(
        byte[] pngBytes, bool noSrgb, Action<string> log)
    {
        return Task.Run(() =>
        {
            string tmpDir = TmpDir();
            try
            {
                string inputPath = Path.Combine(tmpDir, "input.png");
                string stem      = Path.Combine(tmpDir, "output");

                File.WriteAllBytes(inputPath, pngBytes);
                TextureProcessor.ImportPng(inputPath, stem, writeThumb: true, noSrgb, log);

                return ReadOutputs(tmpDir, skip: inputPath);
            }
            finally { Cleanup(tmpDir); }
        });
    }

    /// <summary>.zs bytes → PNG(s)</summary>
    public static Task<List<OutputFile>> ExportZsAsync(
        byte[] zsBytes, string originalFilename, bool noSrgb, Action<string> log)
    {
        return Task.Run(() =>
        {
            string tmpDir = TmpDir();
            try
            {
                string inputPath = Path.Combine(tmpDir, originalFilename);
                File.WriteAllBytes(inputPath, zsBytes);
                TextureProcessor.ExportFileToPngs(inputPath, noSrgb, log);

                return ReadOutputs(tmpDir, skip: inputPath);
            }
            finally { Cleanup(tmpDir); }
        });
    }

    // ── Helpers ──────────────────────────────────────────────

    private static string TmpDir()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static List<OutputFile> ReadOutputs(string dir, string skip)
    {
        var results = new List<OutputFile>();
        foreach (string file in Directory.GetFiles(dir))
        {
            if (string.Equals(file, skip, StringComparison.OrdinalIgnoreCase)) continue;
            results.Add(new OutputFile(Path.GetFileName(file), File.ReadAllBytes(file)));
        }
        return results;
    }

    private static void Cleanup(string dir)
    {
        try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
    }
}

public record OutputFile(string Filename, byte[] Data);
