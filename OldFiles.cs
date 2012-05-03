using System.IO;
using System.Linq;
using System.Security.Cryptography;
using RT.Util;

namespace TankIconMaker
{
    static class OldFiles
    {
        private class oldFile
        {
            public string Name;
            public long Length;
            public string Sha256;

            public oldFile(string name, long length, string sha256)
            {
                Name = name;
                Length = length;
                Sha256 = sha256;
            }
        }

        static oldFile[] _oldFiles = new[]
        {
            new oldFile("Data\\background.jpg", 299329, "f1b3558d82bd35bc8a7ad4396bb086de5ae0c3c4f4664bf3e2ae6554db672a97"),
            new oldFile("Data\\Data-BuiltIn-0.7.0-001.csv", 5280, "b85c96aee01f811e13d00e814e7040e3377e0bb5fb550bf60ee1eda8a326bbea"),
            new oldFile("Data\\Data-BuiltIn-0.7.1-001.csv", 745, "dd2a659389e63227e44328f727b6a2901ce68e921cf15a1cc084b9d61f7ef822"),
            new oldFile("Data\\Data-BuiltIn-0.7.1-001.csv", 6007, "abb845a99208c132e40c5a8c8036ad6e45acc849fd42d3995be228771a74443b"),
            new oldFile("Data\\Data-BuiltIn-0.7.2-001.csv", 406, "4d98414a5109eb4952b8ac8f7cd0b834455f330bd929ab1958ca4fbdf4dfa555"),
            new oldFile("Data\\Data-BuiltIn-0.7.2-001.csv", 6247, "4de8695c3993280a1a59c9ad969b4d442da4491cdfead0887c414421e2c61491"),
            new oldFile("Data\\Data-NameFullWG-En-Romkyns-0.7.0-001.csv", 3346, "6563eee7572e76d533ba437c2da48e3a18da5def765b90a0289518768b403cbd"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.0-001.csv", 3484, "6f9fee97c99ace5a9cd387888fc5a2080b9047b59b947d236e29abd4c465214f"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.1-001.csv", 3971, "7143789c0acb6e062c2617521f0d4c5b4b53a62544365b4d818275b03318fd33"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.2-001.csv", 327, "0eef5d2126c05e90f60198a816423f04c0870a400970851273f2ab5de44c9757"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.7.1-001.csv", 506, "991a602ca33b222834f38a5a1f1c1337bcb018a309e84eb0b0285e50d200fc7d"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.7.1-001.csv", 526, "1cd05b8bba22ba75a90f01a52b2ed29d59f7cb4ad16fa6aec9dd2638cd2ec991"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.7.1-001.csv", 1326, "e76f9d0ef7a4eafce721e4125c43180da24e7e9705bef159f36c57ba0cd2bac4"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3421, "b732bb1334be8d7aff9e9a18203b0d929d17df59aba0f294971d979f5f23d273"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3391, "4f453e9e84fe61c97b9134851abdb4e3d17502e034096a8d46e56688e2c1d9dc"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3369, "67fb30387018299f4dd49d2fb41398cd722864d24a8d3869e1fe0bf078918d38"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.1-001.csv", 575, "a4f3a3712cbb9579f3e55693ad53646425f1736a83126f67d4a57f3b872eae29"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.1-001.csv", 3754, "2edb5e6cda9755a5b4570a2c40913c24da8ea006253815cd835e3687092baf7b"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.2-001.csv", 452, "76c67066785e9d43d6c8b32b0e9eebcac3c04627cf4ee6ec68b76db925b5adab"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 1405, "c81892462a35b3be20ab17b25f20b022934108a83fddd346366eaab1f73d7708"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 394, "00c2ccf728202d4b62de726499b92b53cffde2af554fb4bada515cdc0ca31ec6"),
            new oldFile("Data\\GameVersion-0.7.0.xml", 318, "448f45e8452f4839fe20f5f4e5a01e67de237384fbdf0ad3c37d1a8295462bb7"),
            new oldFile("Data\\GameVersion-0.7.1.xml", 326, "3b82b009159f234c86239a930b902d975a7ba11c0c065bc0dda44364972c7d42"),
            new oldFile("Data\\GameVersion-0.7.1.xml", 494, "92ddd672a4316aa1dd8c61a154175600f539b23c0c28fb4896c355ebcf09f3f6"),
            new oldFile("Data\\GameVersion-0.7.2.xml", 502, "bf45aeed4422e0f0af5d8c9305f72a832209486fedd0a3a62ed23da82505437a"),
            new oldFile("Data\\GameVersion-0.7.2.xml", 526, "deee99d7dac1967830e716d0a2f60ad63de65ca1cea883a71f0d348e5b8aebf2"),
        };

        static string[] _currentFiles = new[]
        {
            "Data\\background.jpg",
            "Data\\Data-BuiltIn-0.7.3-001.csv",
            "Data\\Data-NameFullWG-En-Romkyns-0.0.0-001.csv",
            "Data\\Data-NameFullWG-Ru-Romkyns-0.0.0-001.csv",
            "Data\\Data-NameImproved-Ru-Romkyns-0.0.0-001.csv",
            "Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv",
            "Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv",
            "Data\\Data-NameSlang-Ru-Romkyns-0.0.0-001.csv",
            "Data\\GameVersion-0.7.3.xml",
        };

        public static void DeleteOldFiles()
        {
            foreach (var file in _oldFiles.Where(of => !_currentFiles.Any(cf => cf.EqualsNoCase(of.Name))).GroupBy(of => of.Name))
            {
                try
                {
                    var name = Path.Combine(PathUtil.AppPath, file.Key);
                    if (!File.Exists(name))
                        continue;
                    long length = new FileInfo(name).Length;
                    if (!file.Any(of => of.Length == length))
                        continue;
                    string hash;
                    using (var stream = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read))
                        hash = SHA256.Create().ComputeHash(stream).ToHex();
                    if (!file.Any(of => of.Length == length && of.Sha256.EqualsNoCase(hash)))
                        continue;
                    File.Delete(name);
                }
                catch { } // errors are unimportant; just leave the file alone if anything at all goes wrong
            }
        }
    }
}
