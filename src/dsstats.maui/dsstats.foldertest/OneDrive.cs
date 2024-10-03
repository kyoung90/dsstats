
namespace dsstats.foldertest;

public static class OneDriveFolderTest
{
    public static void GetOneDriveFiles()
    {
        var folder = @"C:\Users\pax77\OneDrive\input";

        var files = Directory.GetFiles(folder);

        foreach (var file in files)
        {
            Console.WriteLine(file);
            var fileInfo = new FileInfo(file);
            var bytes = File.ReadAllBytes(file);
            Console.WriteLine($"\t{fileInfo.Exists} {fileInfo.Length} {bytes.Length}");
        }
    }
}