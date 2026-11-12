using System;
using System.IO;

public static class SaveFileStoreChecks
{
    private static void Check(bool result, string message)
    {
        if (!result) throw new Exception(message);
    }
    public static void Main()
    {
        string directory = Path.Combine(Environment.CurrentDirectory, "Temp", "SaveChecks-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "progress.json");
        SaveFileStore.Write(path, "{\"Cash\":500,\"Label\":\"\u00A5\"}");
        string first = SaveFileStore.Read(path);
        Check(first.Contains("500") && first.Contains("\u00A5"), "First save and UTF-8 round trip");
        SaveFileStore.Write(path, "{\"Cash\":600}");
        Check(SaveFileStore.Read(path) == "{\"Cash\":600}", "Replacement save");
        Check(SaveFileStore.Read(path + ".bak") == first, "Backup preserves previous save");
        SaveFileStore.Write(path, "{\"Cash\":700}");
        Check(SaveFileStore.Read(path + ".bak") == "{\"Cash\":600}", "Backup rotates on subsequent save");
        bool rejected = false;
        try { SaveFileStore.Write(path, ""); } catch (ArgumentException) { rejected = true; }
        Check(rejected && SaveFileStore.Read(path) == "{\"Cash\":700}", "Invalid write does not destroy current progress");
        Check(!File.Exists(path + ".tmp"), "Successful saves leave no temporary file");
        string invalid = Path.Combine(directory, "destination-is-a-directory");
        Directory.CreateDirectory(invalid);
        rejected = false;
        try { SaveFileStore.Write(invalid, "{\"Cash\":800}"); } catch (IOException) { rejected = true; }
        Check(rejected && !File.Exists(invalid + ".tmp"), "Failed writes clean temporary files");
        Check(SaveFileStore.Read(path) == "{\"Cash\":700}", "Failed writes preserve unrelated save");
        Console.WriteLine("PASS: first save, UTF-8 round trip, atomic replacement, backup rotation, rejected empty write, failed write cleanup and preservation.");
    }
}
