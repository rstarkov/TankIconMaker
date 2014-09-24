﻿using System.IO;
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
            new oldFile("Data\\Data-BuiltIn-#0332-1.csv", 9624, "86909eaafdcf1c9985ed6ab2f682ba7421b62e3a7d112c2042a1e7e830aac803"),
            new oldFile("Data\\Data-BuiltIn-#0352-1.csv", 10328, "b5a09644003e2a2ef2c41776959721049721002a9f15c1af078b64b0574cbf37"),
            new oldFile("Data\\Data-BuiltIn-#0352-1.csv", 10392, "37d28295b9f012a46025013cdeea173d1a85b2a32eda42e9bd6ccbc6b18d9d6f"),
            new oldFile("Data\\Data-BuiltIn-#0381-1.csv", 10900, "adadc8908231add6fb483aa16276a99ecb5e524dccb3a680de94d921f847a664"),
            new oldFile("Data\\Data-BuiltIn-#0381-1.csv", 10964, "64d1a1046e9374f77ea34ece5d06b8328ccc90755884e2fb2407416fd3340d36"),
            new oldFile("Data\\Data-BuiltIn-#0405-1.csv", 11472, "2e496c6e472473c352347cbbdd41b715593d347c3727fc78e22eb3ae2055e997"),
            new oldFile("Data\\Data-BuiltIn-#0439-1.csv", 11941, "371fa38063e4f4d211dbdf733686544113fc7f983668c22b1d7b1755841e67c9"),
            new oldFile("Data\\Data-BuiltIn-#0466-1.csv", 12410, "47fc8c131e894efd2e1b07f0b8337bbd1237b1496621e552a9bf9232d133482a"),
            new oldFile("Data\\Data-BuiltIn-#0515-1.csv", 12778, "28d8d0e968e7c1d7526796a6361e4f705f641e040689b168a23fae584d67d188"),
            new oldFile("Data\\Data-BuiltIn-#0515-1.csv", 12778, "c1e57dc0bd02b0470ce9b19fe94ddcfaa6772c5c7c580fe58e49532e0ae561a6"),
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
            new oldFile("Data\\Data-BuiltIn-0.7.5-001.csv", 8130, "35ac3f7ad8f2677468ff937386b8e6262071d608128602bba6034670bd8a6626"),
            new oldFile("Data\\Data-BuiltIn-0.7.5-001.csv", 8130, "b790eabcd1245db6f84cd1da3a5a9cc0019191cd7d671eaab5e4bff96ccecaf2"),
            new oldFile("Data\\Data-BuiltIn-0.7.5-001.csv", 8130, "e8e40ddf4ab9ed64a7f0b92f7c32868e88b839983983c8eabace2b74fe7e9ca4"),
            new oldFile("Data\\Data-BuiltIn-0.8.0-001.csv", 8421, "5385e225cae3eaf1a11578a6a0dab63623a2952d4602b014f3dd84058fcf9e06"),
            new oldFile("Data\\Data-BuiltIn-0.8.0-001.csv", 8421, "973b79c7b90bd53289599a672577e3a69266881b39e2c299fa17486a7042f990"),
            new oldFile("Data\\Data-BuiltIn-0.8.1-001.csv", 9438, "ea123e306e013a08b491e6f47ea3d7f416fc0bf1d10b0fccd3705d7784eb63dc"),
            new oldFile("Data\\Data-BuiltIn-0.8.2-001.csv", 9624, "86909eaafdcf1c9985ed6ab2f682ba7421b62e3a7d112c2042a1e7e830aac803"),
            new oldFile("Data\\Data-BuiltIn-0.8.2-001.csv", 10332, "931337acb1ecf79a9ecb41cf337087776aaf69cef9cad275ca0790f75314b171"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0332-1.csv", 6726, "3c9fa40b5acb799b695174cd3b3fff4f4f88badceb2e4031a5982141c892d582"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0352-1.csv", 7222, "f6058e568e34da99d1e962242a8c157413ef2e9592249578a8141cf2e60fd3d9"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0381-1.csv", 7887, "0cdd959bfbee8e0b54246f0d7c78cdab1fc080864b3d477681d0f11a27287631"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0405-1.csv", 8338, "bcd6d4eb8c638fe91c83e114addd656a3f8d694c6832245d61b046cb013d7431"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0439-1.csv", 8479, "d98ada188b3c6814b897c74d5f68376b286578a0f8ff4db6b47ebc60963d12a3"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0466-1.csv", 8814, "68b5dd8280209cb94f636208dd5b213f59e74049de348ef53685becb48f87cab"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-#0515-1.csv", 9084, "9490ba4ea0a7aa4f73a372d668e9139b2664b8728c84cc4a5d11b0c1cd1a4114"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-0.0.0-001.csv", 5700, "35148340a6b77a08406cc71d1cba0fb5a860be0fb5cc9d2eed629d115086b39b"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-0.0.0-001.csv", 6566, "bf0dc9e222602d421af328270d7dbc5c3248e58daf4183ea68894332f29c114b"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-0.0.0-001.csv", 6726, "3c9fa40b5acb799b695174cd3b3fff4f4f88badceb2e4031a5982141c892d582"),
            new oldFile("Data\\Data-NameFull-Ru-Wargaming-0.0.0-001.csv", 7231, "d88ba973e03d73846c5c34f2d5711e4a59b55790fd67b6044069f005244e0e2b"),
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
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-#0332-1.csv", 2576, "d17c315cb26a28090a702736b4eed2cf6ef5d998ff1ac3084f1a601a933908a3"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv", 1391, "2cf3bfef39f7a157b0217f81b7a5918b9b482f9bdc42e7c7cd07ae62889257f7"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv", 1879, "816e5ad3d6e07e540babf8a3dafc8f8defd2d1a3b1d90c051419766cb91ff6ce"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv", 1909, "982f6e41411be1045d08f03f0f2603262cc57454ebce6d67f73169e47ddcaa6a"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv", 1912, "07d3947f41e397b299648fd9e48394ab33782dceac1ae28fe5b10486079cff4c"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.0.0-001.csv", 2576, "d17c315cb26a28090a702736b4eed2cf6ef5d998ff1ac3084f1a601a933908a3"),
            new oldFile("Data\\Data-NameNative-Ru-Romkyns-0.7.1-001.csv", 1326, "e76f9d0ef7a4eafce721e4125c43180da24e7e9705bef159f36c57ba0cd2bac4"),
            new oldFile("Data\\Data-NameShort-Ru-aboroda-#0332-1.csv", 4860, "5d4cb27dcce2aaa1dcffb6ff9672f2b84f16138982cc700e7e3abfc89c670bfa"),
            new oldFile("Data\\Data-NameShort-Ru-aboroda-0.0.0-001.csv", 4860, "5d4cb27dcce2aaa1dcffb6ff9672f2b84f16138982cc700e7e3abfc89c670bfa"),
            new oldFile("Data\\Data-NameShort-Ru-aboroda-0.0.0-001.csv", 4863, "3a859c8fd8006d1497fbe3bbb1104f315997685e6ba7664abfd520cf2e1f8c2d"),
            new oldFile("Data\\Data-NameShort-Ru-Romkyns-#0332-1.csv", 1533, "f40c98521e786847d1b4651840fdd8ccbc9565e7e2528ce02d5af653d63442c1"),
            new oldFile("Data\\Data-NameShort-Ru-Romkyns-0.0.0-001.csv", 1356, "750e8ad4ac2c59fc555e1261690ed8531377ed13ed038c952ae0e6d9dc6a42f8"),
            new oldFile("Data\\Data-NameShort-Ru-Romkyns-0.0.0-001.csv", 1533, "f40c98521e786847d1b4651840fdd8ccbc9565e7e2528ce02d5af653d63442c1"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0332-1.csv", 6069, "d11c8fbb7da26772cf7fa5a98aecf36909b8ce61b7edd4a819fc689ea3b321d2"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0352-1.csv", 6531, "a81d1b470a36a1e12fb718b9ed137b3db255609a990e8ff4822b4284a91b7fa6"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0352-1.csv", 6579, "7741d9cf2cb9ae46b643e03bf6e43b3d0285252846f7dffefc0f6c1f2c9e1ef0"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0381-1.csv", 7110, "d8b372094f8e627d0a044ddd140a3fd522d9cfba39c70a28d72b5be116bca46b"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0381-1.csv", 7158, "894e8e00f02ad309cde7f875b841d956128c257d77dceb13e5f380d06d80e1c5"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0405-1.csv", 7578, "8f78342a6d2c6c3b39d1026da82799bb7d36874fd56449181d57c2c0f58d48dc"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0439-1.csv", 7649, "5ae9246e98f72ef9a3cb1f4276094c53a76b2f8af36519f820b2c6bce4aea1c8"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0466-1.csv", 7955, "7f52e73060a10242a0eef7112d8f919859aaf3df1f20798ec360684ee9664d4b"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-#0515-1.csv", 8204, "31d1659c6a4323e8504e777516f8e9aada325f4ba30bb400145e515c6a012af1"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-0.0.0-001.csv", 5109, "715b85dc8fe58394911d122f06d27b9e7f1d64e5b7327b1fc6978f53fb298434"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-0.0.0-001.csv", 5827, "08551be08048da202b7fa13b197a59ebadcd539172270e88737b3c1cfa0230be"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-0.0.0-001.csv", 6069, "d11c8fbb7da26772cf7fa5a98aecf36909b8ce61b7edd4a819fc689ea3b321d2"),
            new oldFile("Data\\Data-NameShort-Ru-Wargaming-0.0.0-001.csv", 6534, "2f84096582cf29df3c295dc1ee230e00d6894e8fdf25825580e4fda0bd3c3664"),
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
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-#0332-1.csv", 394, "00c2ccf728202d4b62de726499b92b53cffde2af554fb4bada515cdc0ca31ec6"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.0.0-001.csv", 394, "00c2ccf728202d4b62de726499b92b53cffde2af554fb4bada515cdc0ca31ec6"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 394, "00c2ccf728202d4b62de726499b92b53cffde2af554fb4bada515cdc0ca31ec6"),
            new oldFile("Data\\Data-NameSlang-Ru-Romkyns-0.7.1-001.csv", 1405, "c81892462a35b3be20ab17b25f20b022934108a83fddd346366eaab1f73d7708"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0332-1.csv", 4750, "06e770b1b8dc7d72e331d3eb23ca008df5d5233dd057ab6084acd418e253f4d7"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0352-1.csv", 5140, "c183e2061ca1f0dba29923fdc385e73ca4a4ad8ef5bba4a6e844960f23fb9161"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0381-1.csv", 5616, "0d29954922048300219ce6e3112e5a7e58ed9581cfbd2f290f93684742458eba"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0405-1.csv", 5888, "3626456b0b86c4025fffec58294191c377f852ff8eef5c6ddf04f323e6876e59"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0439-1.csv", 5978, "9d0ac15f5fc03f7aee2b6672539176f9bd96b2cde799df8d3a91f221c9481f03"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0466-1.csv", 6227, "a0e6c30a070d0c1bd864c6dd5c680d806c91f1b8de667af6d9daf494ab42fc25"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-#0515-1.csv", 6418, "93ebbdc2c8ec7a5e163e6f65fb0273c1981368a608f2d6238d99bfca4832f490"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-0.0.0-001.csv", 4669, "7d7dbafa3f4cf0a352ea8524b048c58db3abb26eec5d6e417304bc265d6bc9af"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-0.0.0-001.csv", 4750, "06e770b1b8dc7d72e331d3eb23ca008df5d5233dd057ab6084acd418e253f4d7"),
            new oldFile("Data\\Data-SpeedForward-X-Romkyns-0.0.0-001.csv", 5144, "15f80e65f3904acf400a5800695cdb3b62a7d7d117d66abffd4486515340aebf"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0332-1.csv", 4731, "4e3c0dcf6b29bab8b608075939f7a720aceb01b72336886899ff84d236570f4a"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0352-1.csv", 5121, "ac7458c05775a7a4bcf95d10a6da8072ab05af9d9c64fac76ade492adffb3f98"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0381-1.csv", 5594, "67ced3be3865bd1abdb8e5b939f7851de57e4c0b1a44dc2b2ee1e84a3b9d22f4"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0405-1.csv", 5866, "268bd6dc58d207aef96e9bf4a9fcbf5c94a9874a71b19f31d6c58df5ed7a1395"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0439-1.csv", 5894, "27482dabcf9e84bb60a7e9ae2ec4745bfc56c91a93a3faad854b7aab959cb62c"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0466-1.csv", 6137, "04cbb53cf4a8c41dd7cbe008d7a7022ec74c6a8d659a90b03b79d6fac69a4fa7"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-#0515-1.csv", 6316, "5c19c9db0d065ec361a2fd3b5100d2e6da45f4797bc6b79d7c437a7c1165b8fc"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-0.0.0-001.csv", 4650, "49ef40873c9e5a3d60c1974414a5707964a4d623cea51b9e82520c102f71e434"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-0.0.0-001.csv", 4731, "4e3c0dcf6b29bab8b608075939f7a720aceb01b72336886899ff84d236570f4a"),
            new oldFile("Data\\Data-SpeedReverse-X-Romkyns-0.0.0-001.csv", 5124, "89e6a09e989fdf678888b058cd931c5e76e4c6b3afc7f2e4005bbc2455d723aa"),
            new oldFile("Data\\GameVersion-#0007.xml", 1649, "c72f3e423ed72f6d50ddd2da371e06d48087968768868e558e29f6ac3152c87e"),
            new oldFile("Data\\GameVersion-#0007.xml", 1649, "88a3431ac701d45ff966870c2466589062c4e11803d9427a41589d76c2750e16"),
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
            new oldFile("Data\\GameVersion-0.7.5.xml", 1803, "48ba7aeb18533c942b4cce679b6f761d54c82ee0995e988b1661101e573eb97e"),
            new oldFile("Data\\GameVersion-0.7.5.xml", 1804, "6c9eb56ff55b08b7d2a44a87b9c55e556fc3ea15b37e09d8ed77e4d8cdf4f184"),
            new oldFile("Data\\GameVersion-0.7.5-CT.xml", 1747, "db41eb742a20502083af0d9793016737ccccd33b9a37e3b876b3f656e0054115"),
            new oldFile("Data\\GameVersion-0.8.0.xml", 1804, "9b430e3a922c62578708f21999331707ea2bdcfada190b970b1ea17a0c15319f"),
            new oldFile("Data\\GameVersion-0.8.0.xml", 1804, "f7db9b83f15aecb7336ea56acf43627c544c959c2eb0008921b37d23bdefce44"),
            new oldFile("Data\\GameVersion-0.8.0-CT.xml", 1833, "1e5c81f823c028d0d31072994da4fa096ce436550031ef381443eb5acbce7d38"),
            new oldFile("Data\\GameVersion-0.8.1.xml", 1804, "b071061273675cba8bf0cf2460635afa54a706bfde9f1acc78c314568a4cb799"),
            new oldFile("Data\\GameVersion-0.8.1-CT.xml", 1833, "35eff4198fe4434d1c634168087606793a595da3ce14aa802f5f18660977ae68"),
            new oldFile("Data\\GameVersion-0.8.2.xml", 1804, "1233a2a49e4396fca73b7b6ffaf112a411306cc7cc5f255384f4ccff5daf0ea9"),
            new oldFile("Data\\GameVersion-0.8.2.xml", 1804, "f4db1782e5811697f5051d216b1f88e859378f8dfcfee95e69f72b7da361c329"),
            new oldFile("Data\\GameVersion-0.8.2-CT.xml", 1833, "08c873fc6e7646c8ddaa8209eb9d5e788acf54ccd285e410276d62e7f54ae5f6"),
            new oldFile("Data\\Versions.csv", 426, "c394e4e4a8f4fe9dbc184f5dab3394a9b805f28757913f3321b32dfbf74e6f70"),
            new oldFile("Data\\Versions.csv", 468, "f79360687b19a96dad7d06a87fc9c089a47c144f7eab76d1ce33dad8a76fe632"),
            new oldFile("Data\\WotGameVersion-#0007.xml", 1880, "2ea54454a6267eecf6ac94289434517ecddcb60e7040b48c0f040ba2fddc78f7"),
            new oldFile("Data\\WotGameVersion-#0007.xml", 1880, "5eba3f47fdce0d72e98df87f45f2dc90a4df5b9e402b1cd0121d3659333acc05"),
            new oldFile("Data\\WotGameVersion-#0150.xml", 1880, "2ea54454a6267eecf6ac94289434517ecddcb60e7040b48c0f040ba2fddc78f7"),
            new oldFile("Data\\WotGameVersion-#0500.xml", 1843, "a50848e7fe90381a1ed228c09cfeed1643dd4aff7ff8154ad4df56ef0d0894be"),
            new oldFile("license-ookii-dialogs.txt", 1569, "f9e2eb02b8ff3ebe1f95cd604082f62a6f33215b8832face93142b24398b610d"),
            new oldFile("license-rt-util.txt", 33093, "321429c853d59b621414832581e93ff38586c348aa2de43c7e571b33423d670a"),
            new oldFile("license-sharpziplib.txt", 19463, "902ab97972b67b826ab8caba382345f44fa70e561dc5cd3eff5cb51d3df4eac4"),
            new oldFile("license-tank-icon-maker.txt", 33093, "321429c853d59b621414832581e93ff38586c348aa2de43c7e571b33423d670a"),
            new oldFile("license-wpf-toolkit.txt", 2659, "3907b72d92cd01809f502fd5d583ad9c1ef9a1e924c4beb91d6d96e9c68a6e81"),
        };

        static string[] _currentFiles = new[]
        {
            "Backgrounds/Abbey (Монастырь).jpg",
            "Backgrounds/Karelia (Карелия).jpg",
            "Backgrounds/Lakeville (Ласвилль).jpg",
            "Backgrounds/Ruinberg (Руинберг).jpg",
            "Backgrounds/Sand River (Песчаная Река).jpg",
            "Backgrounds/Westfield (Вестфилд).jpg",
            "Data/WotBuiltIn-1.csv",
            "Data/WotData-NameNative-Romkyns-1.csv",
            "Data/WotData-NameNative-Seriych-1.csv",
            "Data/WotData-NameSlang-Romkyns-1.csv",
            "Data/WotGameVersion-#0039.xml",
            "ICSharpCode.SharpZipLib.dll",
            "Images/background-slesh-1.png",
            "Images/background-slesh-2.png",
            "Images/background-slesh-3.png",
            "Images/background-slesh-4.png",
            "Images/background-slesh-5.png",
            "Images/background-slesh-6.png",
            "Images/background-slesh-7.png",
            "Images/background-slesh-8.png",
            "Images/class1-Artillery.png",
            "Images/class1-Destroyer.png",
            "Images/class1-Heavy.png",
            "Images/class1-Light.png",
            "Images/class1-Medium.png",
            "Images/class2-Artillery.png",
            "Images/class2-Destroyer.png",
            "Images/class2-Heavy.png",
            "Images/class2-Light.png",
            "Images/class2-Medium.png",
            "Images/class3-Artillery.png",
            "Images/class3-Destroyer.png",
            "Images/class3-Heavy.png",
            "Images/class3-Light.png",
            "Images/class3-medium.png",
            "Images/country1-china.png",
            "Images/country1-france.png",
            "Images/country1-germany.png",
            "Images/country1-japan.png",
            "Images/country1-uk.png",
            "Images/country1-usa.png",
            "Images/country1-ussr.png",
            "Images/country2-china.png",
            "Images/country2-france.png",
            "Images/country2-germany.png",
            "Images/country2-japan.png",
            "Images/country2-uk.png",
            "Images/country2-usa.png",
            "Images/country2-ussr.png",
            "Images/country3-china.png",
            "Images/country3-france.png",
            "Images/country3-germany.png",
            "Images/country3-japan.png",
            "Images/country3-uk.png",
            "Images/country3-usa.png",
            "Images/country3-ussr.png",
            "Images/country4-china.png",
            "Images/country4-france.png",
            "Images/country4-germany.png",
            "Images/country4-japan.png",
            "Images/country4-uk.png",
            "Images/country4-usa.png",
            "Images/country4-ussr.png",
            "Images/country5-china.png",
            "Images/country5-france.png",
            "Images/country5-germany.png",
            "Images/country5-japan.png",
            "Images/country5-uk.png",
            "Images/country5-usa.png",
            "Images/country5-ussr.png",
            "Images/country6-china.png",
            "Images/country6-france.png",
            "Images/country6-germany.png",
            "Images/country6-japan.png",
            "Images/country6-uk.png",
            "Images/country6-ussr.png",
            "Images/misc-squad.png",
            "Images/sources.txt",
            "Images/tierA-01.png",
            "Images/tierA-02.png",
            "Images/tierA-03.png",
            "Images/tierA-04.png",
            "Images/tierA-05.png",
            "Images/tierA-06.png",
            "Images/tierA-07.png",
            "Images/tierA-08.png",
            "Images/tierA-09.png",
            "Images/tierA-10.png",
            "license-ICSharpCode.SharpZipLib.txt",
            "license-Ookii.Dialogs.Wpf.txt",
            "license-RT.Util.txt",
            "license-TankIconMaker.txt",
            "license-WpfCrutches.txt",
            "license-WPFToolkit.Extended.txt",
            "Ookii.Dialogs.Wpf.dll",
            "RT.Util.dll",
            "TankIconMaker.exe",
            "TankIconMaker.pdb",
            "Translations/TankIconMaker.de.xml",
            "Translations/TankIconMaker.ru.xml",
            "WotDataLib.dll",
            "WotDataLib.pdb",
            "WpfCrutches.dll",
            "WpfCrutches.pdb",
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
