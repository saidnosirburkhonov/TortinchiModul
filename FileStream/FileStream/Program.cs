namespace FileStream;

internal class Program
{
    static async Task Main(string[] args)
    {   //var path = @"D:\salom.txt";
        //var chunkSize = 1024 * 1024 * 10;
        //using (var fs = new FileStream(path, FileMode.Open))
        //{
        //    byte[] buffer = new byte[chunkSize];

        //    int bytesRead = fs.Read(buffer, 0, buffer.Length);

        //    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        //    Console.WriteLine(text);
        //}


        //using (var fs = new FileStream(path, FileMode.Create))
        //{
        //    string text = "Salom bro!";
        //    byte[] bytes = Encoding.UTF8.GetBytes(text);
        //    //bytes[1] = 65;
        //    //bytes[5] = (byte)'@';

        //    fs.Write(bytes, 0, bytes.Length);
        //}
        string sourcePath = @"D:\OnlineCource\2026-03-26 14-49-17.mp4";
        string destinationPath = @"D:\OnlineCource\Test\dars2.mp4";

        await CopyLargeFileAsync(sourcePath, destinationPath);
    }
    public static async Task CopyLargeFileAsync(string sourcePath, string destinationPath)
    {
        const int bufferSize = 1024 * 1024 * 10; // 10 MB

        using (FileStream sourceStream = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            useAsync: true))

        using (FileStream destinationStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            useAsync: true))
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destinationStream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}
