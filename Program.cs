using System;
using System.IO;
using LibHac;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;

namespace hac
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: \"hac path/to/update/folder/\"");
                return;
            }

            string path = args[0];

            bool dev = args.Length > 1 && args[1] == "dev";

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Path {path} not found");
                return;
            }

            string keyFileName = dev ? "dev.keys" : "prod.keys";

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string keyFile = Path.Combine(home, ".switch", keyFileName);

            if (!File.Exists(keyFile))
            {
                Console.WriteLine($"Keyfile {keyFile} not found");
                return;
            }

            Keyset keyset = null;
            keyset = ExternalKeyReader.ReadKeyFile(keyFile, null, null, null, dev);

            SwitchFs switchFs = SwitchFs.OpenNcaDirectory(keyset, new LocalFileSystem(path));

            foreach (var nca in switchFs.Ncas)
            {
                if (nca.Value.Nca.Header.ContentType == NcaContentType.Meta)
                {
                    var src = Path.Combine(path, $"{nca.Key}.nca");
                    var dst = Path.Combine(path, nca.Value?.Filename);
                    if (File.Exists(src) && !File.Exists(dst))
                    {
                        System.IO.File.Move(src, dst);
                        Console.WriteLine($"Moved {Path.GetFileName(src)} to {Path.GetFileName(dst)}");
                    }
                }
            }
        }
    }
}
