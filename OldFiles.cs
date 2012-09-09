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
            new oldFile("Data\\Data-BuiltIn-0.7.0-001.csv", 5280, "b85c96aee01f811e13d00e814e7040e3377e0bb5fb550bf60ee1eda8a326bbea"),
            new oldFile("Data\\Data-BuiltIn-0.7.1-001.csv", 745, "dd2a659389e63227e44328f727b6a2901ce68e921cf15a1cc084b9d61f7ef822"),
            new oldFile("Data\\Data-BuiltIn-0.7.1-001.csv", 6007, "abb845a99208c132e40c5a8c8036ad6e45acc849fd42d3995be228771a74443b"),
            new oldFile("Data\\Data-BuiltIn-0.7.2-001.csv", 406, "4d98414a5109eb4952b8ac8f7cd0b834455f330bd929ab1958ca4fbdf4dfa555"),
            new oldFile("Data\\Data-BuiltIn-0.7.2-001.csv", 6244, "0a6a69d7942ebcd278e4d90da36ef629f2137f7f89b3e8cef1083e5cdd251503"),
            new oldFile("Data\\Data-BuiltIn-0.7.2-001.csv", 6247, "4de8695c3993280a1a59c9ad969b4d442da4491cdfead0887c414421e2c61491"),
            new oldFile("Data\\Data-BuiltIn-0.7.3-001.csv", 405, "043dc774fbf93d6e0840947eea5208a74f600265b033189b053027a7d843e25d"),
            new oldFile("Data\\Data-BuiltIn-0.7.3-001.csv", 6608, "4eb99e47b81342b26c70128734d9ba5ab35d5e56af5121328af6159d90ec393f"),
            new oldFile("Data\\Data-BuiltIn-0.7.3-001.csv", 6610, "83adabf9719da7447d33c5c6baee3184b4de988487600aaa04c5466a9e5b78d5"),
            new oldFile("Data\\Data-BuiltIn-0.7.3-001.csv", 6877, "40d86b8684d6f5807166076d174493fecbcf45a902b124bf41694080e34f35ea"),
            new oldFile("Data\\Data-BuiltIn-0.7.4-001.csv", 796, "d21493a769f9de7d2795eff48f41c5793bd052560db0d2aba278bf0b935e5469"),
            new oldFile("Data\\Data-BuiltIn-0.7.4-001.csv", 942, "de01b2fc60e6dc8f1b7c007656f2991b5445e3a7ee205a1954d87e8587570583"),
            new oldFile("Data\\Data-BuiltIn-0.7.4-001.csv", 7669, "ca515538e043c6d42d1c28e9cbe0f59f91fce2b32c36cf217390dcf6dfbfe96d"),
            new oldFile("Data\\Data-NameFullWG-En-Romkyns-0.0.0-001.csv", 3346, "6563eee7572e76d533ba437c2da48e3a18da5def765b90a0289518768b403cbd"),
            new oldFile("Data\\Data-NameFullWG-En-Romkyns-0.7.0-001.csv", 3346, "6563eee7572e76d533ba437c2da48e3a18da5def765b90a0289518768b403cbd"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.0.0-001.csv", 4129, "939932000d222ffab4dcf32f65f3286b07a8a14e0362f8b98d35023c53ceeb17"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.0.0-001.csv", 5243, "ec82641fd1e98ba6668f24350da336fd6c74712fd79948b9355eec9e2c7466be"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.0.0-001.csv", 5487, "28b93a11a4d67c53db1d2ee039f54b99c6903ffb006935ea750f9ba43c6dc8c2"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.0-001.csv", 3484, "6f9fee97c99ace5a9cd387888fc5a2080b9047b59b947d236e29abd4c465214f"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.1-001.csv", 3971, "7143789c0acb6e062c2617521f0d4c5b4b53a62544365b4d818275b03318fd33"),
            new oldFile("Data\\Data-NameFullWG-Ru-Romkyns-0.7.2-001.csv", 327, "0eef5d2126c05e90f60198a816423f04c0870a400970851273f2ab5de44c9757"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.0.0-001.csv", 573, "9468f7b5a1a048a9e1c1bc78380cb01315a3c9fe3e47e80863b4ad00c63fa704"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.0.0-001.csv", 969, "1abed4d6a52dccaee6fd4531766aed9f2eb00eba64662dcd72f7fb0fb40a326a"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.0.0-001.csv", 973, "e730d77dda5e5f889202502b3d0106396e29b0f9c5454c61bb2552f548a9ca2a"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.0.0-001.csv", 1356, "5e4371df33d90888889d9e5e5be3421fc04daca2c45624af518275f761eed2fe"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.7.1-001.csv", 506, "991a602ca33b222834f38a5a1f1c1337bcb018a309e84eb0b0285e50d200fc7d"),
            new oldFile("Data\\Data-NameImproved-Ru-Romkyns-0.7.1-001.csv", 526, "1cd05b8bba22ba75a90f01a52b2ed29d59f7cb4ad16fa6aec9dd2638cd2ec991"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.7.1-001.csv", 1326, "e76f9d0ef7a4eafce721e4125c43180da24e7e9705bef159f36c57ba0cd2bac4"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 3856, "fb8cc38bb3554e193a310a53911d5cb08b7705ea50f75d83f8009cb2276df56e"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4247, "75470f35a89517b6c7ae55dc6c57c25308751d9918c7defbe177589e2ca63af7"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4270, "ce38a3020cc67734270216b057769b362ec268386bd884ac03f85e5863d21b1e"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4280, "973c9a89b9c85d8107237bac57fbef63c0a69704dbdfb7788e8fa69737316060"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4714, "7852ea9485db78b6ad7f7614089e7537530c796f119ba29777f795664b5aa891"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4919, "82076ed84f7499737a29b3690cd89ae6321cc6ef67fea91a837a982438b54fbd"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.0.0-001.csv", 4920, "83f0eb43ec2f9fd12c3b5d74a3780c6e6ac0be7831d7bb741cf432d82dba7628"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3369, "67fb30387018299f4dd49d2fb41398cd722864d24a8d3869e1fe0bf078918d38"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3391, "4f453e9e84fe61c97b9134851abdb4e3d17502e034096a8d46e56688e2c1d9dc"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.0-001.csv", 3421, "b732bb1334be8d7aff9e9a18203b0d929d17df59aba0f294971d979f5f23d273"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.1-001.csv", 575, "a4f3a3712cbb9579f3e55693ad53646425f1736a83126f67d4a57f3b872eae29"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.1-001.csv", 3754, "2edb5e6cda9755a5b4570a2c40913c24da8ea006253815cd835e3687092baf7b"),
            new oldFile("Data\\Data-NameShortWG-Ru-Romkyns-0.7.2-001.csv", 452, "76c67066785e9d43d6c8b32b0e9eebcac3c04627cf4ee6ec68b76db925b5adab"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 394, "00c2ccf728202d4b62de726499b92b53cffde2af554fb4bada515cdc0ca31ec6"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 1405, "c81892462a35b3be20ab17b25f20b022934108a83fddd346366eaab1f73d7708"),
            new oldFile("Data\\GameVersion-0.7.0.xml", 318, "448f45e8452f4839fe20f5f4e5a01e67de237384fbdf0ad3c37d1a8295462bb7"),
            new oldFile("Data\\GameVersion-0.7.1.xml", 326, "3b82b009159f234c86239a930b902d975a7ba11c0c065bc0dda44364972c7d42"),
            new oldFile("Data\\GameVersion-0.7.1.xml", 494, "92ddd672a4316aa1dd8c61a154175600f539b23c0c28fb4896c355ebcf09f3f6"),
            new oldFile("Data\\GameVersion-0.7.2.xml", 487, "f0469f2caee930c237819377a793f79489b4a375253e6095d55ed6ce296037ae"),
            new oldFile("Data\\GameVersion-0.7.2.xml", 502, "bf45aeed4422e0f0af5d8c9305f72a832209486fedd0a3a62ed23da82505437a"),
            new oldFile("Data\\GameVersion-0.7.2.xml", 526, "deee99d7dac1967830e716d0a2f60ad63de65ca1cea883a71f0d348e5b8aebf2"),
            new oldFile("Data\\GameVersion-0.7.3.xml", 555, "24c05eecf160cffafb0c4c7eca5743c1887851f9fc2a6d3184704fc8f5927f8a"),
            new oldFile("Data\\GameVersion-0.7.3.xml", 1670, "4dda831d8f1a271a313693d30ea4eeee8eda4957994eca401d4cca023836c2a0"),
            new oldFile("Data\\GameVersion-0.7.4.xml", 1695, "fe0dbe30db41169b16a21d6ffb49cf436809c5c75118f36bc3bfdf9ee730419c"),
            new oldFile("Data\\GameVersion-0.7.4.xml", 1717, "433b7d9a05bf3ed81974663ac1e9a59fe85f8016ebaf8e7f91258417044455a2"),
            new oldFile("Data\\GameVersion-0.7.4.xml", 1717, "bc491f830323bd52e7ecac7fd85dfb01ccd40d1a01a1318c652392bd3844af94"),
            new oldFile("Data\\GameVersion-0.7.4-CT.xml", 1746, "cdfb1c839cb388602f7c2dff1f40a80458283e12a0f0ac9af5a9b2f8edcf53f6"),
            new oldFile("Data\\GameVersion-0.7.5-CT.xml", 1747, "db41eb742a20502083af0d9793016737ccccd33b9a37e3b876b3f656e0054115"),
        };

        static string[] _currentFiles = new[]
        {
            "Data/background.jpg",
            "Data/Data-BuiltIn-0.7.5-001.csv",
            "Data/Data-BuiltIn-0.8.0-001.csv",
            "Data/Data-NameFull-Ru-Wargaming-0.0.0-001.csv",
            "Data/Data-NameNative-Ru-Romkyns-0.0.0-001.csv",
            "Data/Data-NameShort-Ru-aboroda-0.0.0-001.csv",
            "Data/Data-NameShort-Ru-Romkyns-0.0.0-001.csv",
            "Data/Data-NameShort-Ru-Wargaming-0.0.0-001.csv",
            "Data/Data-NameSlang-Ru-Romkyns-0.0.0-001.csv",
            "Data/GameVersion-0.7.5.xml",
            "Data/GameVersion-0.8.0.xml",
            "Data/GameVersion-0.8.0-CT.xml",
            "ICSharpCode.SharpZipLib.dll",
            "Images/class1-Artillery.png",
            "Images/class1-Destroyer.png",
            "Images/class1-Heavy.png",
            "Images/class1-Light.png",
            "Images/class1-Medium.png",
            "license-ookii-dialogs.txt",
            "license-rt-util.txt",
            "license-sharpziplib.txt",
            "license-tank-icon-maker.txt",
            "license-wpf-toolkit.txt",
            "Ookii.Dialogs.Wpf.dll",
            "RT.Util.dll",
            "TankIconMaker.exe",
            "TankIconMaker.pdb",
            "Translations/TankIconMaker.de.xml",
            "Translations/TankIconMaker.ru.xml",
            "WPFToolkit.Extended.dll",
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
