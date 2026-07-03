To open and extract an encrypted ZIP file using [SharpZipLib](https://icsharpcode.github.io/SharpZipLib/help/api/ICSharpCode.SharpZipLib.Zip.ZipFile.html) in C#, you must explicitly provide the decryption password to either the ZipFile or ZipInputStream class. Both classic ZipCrypto and strong AES-256 encryptions are supported natively.
The two best approaches depend on whether you are working with local files or streams:
## Method 1: Using ZipFile (Recommended for Local Files)
The ZipFile class allows you to assign a global password to the archive. It handles decryption automatically behind the scenes when you extract the files.

```cs
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public static void UnzipEncryptedFile(string zipFilePath, string targetFolder, string password)
{
    // 1. Open the ZIP archive
    using (FileStream fs = File.OpenRead(zipFilePath))
    using (ZipFile zf = new ZipFile(fs))
    {
        // 2. Assign the password required for extraction
        zf.Password = password;

        // 3. Iterate through each entry in the archive
        foreach (ZipEntry zipEntry in zf)
        {
            // Skip directories (they will be created automatically via file paths)
            if (!zipEntry.IsFile) continue;

            string entryFileName = zipEntry.Name;

            // Construct target file path safely
            string targetFilePath = Path.Combine(targetFolder, entryFileName);
            string directoryName = Path.GetDirectoryName(targetFilePath);

            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // 4. Extract the decrypted file stream
            byte[] buffer = new byte[4096];
            using (Stream zipStream = zf.GetInputStream(zipEntry))
            using (FileStream outputStream = File.Create(targetFilePath))
            {
                int bytesRead;
                while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}
```

## Method 2: Using ZipInputStream (Best for Memory Streams)
If your ZIP archive is stored entirely in memory (e.g., a MemoryStream downloaded over the web), use ZipInputStream. Crucial rule: Ensure you attach the password to the ZipInputStream object before calling GetNextEntry().

```cs
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public static void UnzipEncryptedStream(Stream sourceStream, string targetFolder, string password)
{
    using (ZipInputStream zipStream = new ZipInputStream(sourceStream))
    {
        // Assign password prior to scanning entries
        zipStream.Password = password;

        ZipEntry entry;
        while ((entry = zipStream.GetNextEntry()) != null)
        {
            if (!entry.IsFile) continue;

            string targetFilePath = Path.Combine(targetFolder, entry.Name);
            string directoryName = Path.GetDirectoryName(targetFilePath);

            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // Write the decrypted data to disk
            byte[] buffer = new byte[4096];
            using (FileStream outputStream = File.Create(targetFilePath))
            {
                int bytesRead;
                while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outputStream.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}
```

## ⚠️ Troubleshooting Common Pitfalls

* Mixing Classes: Do not combine ZipInputStream and ZipFile logic together (e.g., reading entries via the stream but providing a password to the file configuration). This causes the library to ignore the password and crash.
* Empty Password String: SharpZipLib has a documented quirk where setting .Password = "" behaves the same as null, throwing a "No password available" exception on AES streams.

---

Yes, you can absolutely get a complete list of file names, sizes, and directory structures from an encrypted ZIP file without providing a password.
By design, the standard ZIP format only encrypts the contents of individual files; it does not encrypt the metadata stored in the Central Directory. As a result, [SharpZipLib](https://icsharpcode.github.io/SharpZipLib/help/api/ICSharpCode.SharpZipLib.Zip.ZipFile.html) allows you to iterate over the archive and read metadata freely. A password is only required the exact moment you try to open the InputStream to extract or read the data inside those files.
## Code Example: Listing Metadata Without a Password
You can use the ZipFile class to quickly read the archive index. You can also safely check the IsCrypted property to determine if a specific file requires a password before extraction.

```cs
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

public static void ListZipContents(string zipFilePath)
{
    // Open the file normally; no password is provided here
    using (FileStream fs = File.OpenRead(zipFilePath))
    using (ZipFile zf = new ZipFile(fs))
    {
        Console.WriteLine($"Total Entries Found: {zf.Count}\n");
        Console.WriteLine($"{"File Name",-40} | {"Compressed Size",-15} | {"Is Encrypted?",-12}");
        Console.WriteLine(new string('-', 75));

        foreach (ZipEntry entry in zf)
        {
            // Directories don't have file sizes or individual encryption flags
            if (entry.IsDirectory)
            {
                Console.WriteLine($"[DIR]  {entry.Name}");
                continue;
            }

            // Read metadata properties safely without a password
            string name = entry.Name;
            long size = entry.CompressedSize;
            bool isEncrypted = entry.IsCrypted; // Returns true if password-protected

            Console.WriteLine($"{name,-40} | {size,-15} | {isEncrypted,-12}");
        }
    }
}
```

## Important Limitations to Note

* No Content Reading: If you attempt to call zf.GetInputStream(entry) or use .Read() on any file where IsCrypted is true, SharpZipLib will throw a PasswordRequiredException or a decryption error.
* The "Double Zip" Exception: The only way filenames are completely hidden in a ZIP workflow is if someone compressed the files into a standard archive first, and then zipped/encrypted that entire .zip file into another .zip file. In that specific scenario, you would only be able to see the name of the inner .zip file, but not its internal components.