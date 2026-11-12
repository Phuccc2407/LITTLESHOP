using System;
using System.IO;
using System.Text;

// No scene or Unity dependencies; disk handling can be tested independently.
public static class SaveFileStore
{
    public static void Write(string path, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Save data cannot be empty.");
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        string temporary = path + ".tmp";
        try
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(json);
            using (FileStream stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public static string Read(string path) { return File.ReadAllText(path, Encoding.UTF8); }
}
