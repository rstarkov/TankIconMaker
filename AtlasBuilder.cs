using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Windows.Forms;
using RT.Util.Dialogs;
using ImageMagick;
using TeximpNet.Compression;
using TeximpNet;
using WotDataLib;

namespace TankIconMaker
{
    class AtlasBuilder
    {
        public const string battleAtlas = "BattleAtlas";
        public const string vehicleMarkerAtlas = "vehicleMarkerAtlas";
        public const string customAtlas = "IconsAtlas";

        public static string GetAtlasFilename(SaveType savetype)
        {
            switch (savetype)
            {
                case SaveType.BattleAtlas:
                    return AtlasBuilder.battleAtlas + ".png";
                case SaveType.VehicleMarkerAtlas:
                    return AtlasBuilder.vehicleMarkerAtlas + ".png";
                case SaveType.CustomAtlas:
                    return AtlasBuilder.customAtlas + ".png";
                default:
                    throw new ArgumentOutOfRangeException("savetype");
            }
        }

        private WotContext context;
        private int HeighAtlas = 0;
        public AtlasBuilder(WotContext CurContext)
        {
            context = CurContext;
        }

        private struct SubTextureStruct
        {
            public string FName;
            public System.Drawing.Bitmap ImageTank;
            public System.Drawing.Rectangle LocRect;
            public int MaxParty;


            public SubTextureStruct(string FName, System.Drawing.Bitmap ImageTank, System.Drawing.Rectangle LocRect, int MaxParty)
            {
                this.FName = FName;
                this.ImageTank = ImageTank;
                this.LocRect = LocRect;
                this.MaxParty = MaxParty;
            }
        }

        private void CreateAtlasXML(ref List<SubTextureStruct> ImageList, string filename)
        {
            FileStream fileXML = new FileStream(filename, FileMode.Create);
            StreamWriter writer = new StreamWriter(fileXML);
            writer.WriteLine("<root>");
            foreach (var SubTexture in ImageList)
            {
                writer.WriteLine("  <SubTexture>");
                writer.WriteLine("    <name> " + SubTexture.FName + " </name>");
                writer.WriteLine("    <x> " + (int)SubTexture.LocRect.X + " </x>");
                writer.WriteLine("    <y> " + (int)SubTexture.LocRect.Y + " </y>");
                writer.WriteLine("    <width> " + (int)SubTexture.LocRect.Width + " </width>");
                writer.WriteLine("    <height> " + (int)SubTexture.LocRect.Height + " </height>");
                writer.WriteLine("  </SubTexture>");
            }
            writer.Write("</root>");
            writer.Close();
        }

        private void CreateAtlasImage(ref List<SubTextureStruct> ImageList, string filename)
        {
            if (HeighAtlas <= 0) return;
            System.Drawing.Bitmap AtlasPNG = new System.Drawing.Bitmap(4096, HeighAtlas);
            AtlasPNG.SetResolution(96.0F, 96.0F);
            for (int i = 0; i < ImageList.Count; i++)
            {
                System.Drawing.Bitmap PNG = ImageList[i].ImageTank;
                using (System.Drawing.Graphics gPNG = System.Drawing.Graphics.FromImage(AtlasPNG))
                {
                    gPNG.DrawImage(PNG, (int)ImageList[i].LocRect.X, (int)ImageList[i].LocRect.Y);
                }
            }
            AtlasPNG.Save(filename);
            try
            {
                Surface AtlasDDS = Surface.LoadFromFile(filename, true);
                using (Compressor compressor = new Compressor())
                {
                    compressor.Input.SetData(AtlasDDS);
                    compressor.Input.SetMipmapGeneration(false);
                    compressor.Compression.Format = CompressionFormat.DXT5;
                    compressor.Compression.Quality = CompressionQuality.Production;
                    compressor.Process(filename.Replace(".png", ".dds"));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "CreateAtlasImage", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void RadixSort(ref List<SubTextureStruct> ImageList)
        {
            int CInd;
            int[] C = new int[10];
            List<SubTextureStruct> ImageListTemp = new List<SubTextureStruct>(ImageList);
            int t = 1;
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 0; j < 10; j++)
                    C[j] = 0;
                for (int j = 0; j < ImageList.Count; j++)
                {
                    CInd = (ImageList[j].MaxParty % (t * 10)) / t;
                    C[CInd] = C[CInd] + 1;
                }
                for (int j = 8; j >= 0; j--)
                    C[j] = C[j + 1] + C[j];
                for (int j = ImageList.Count - 1; j >= 0; j--)
                {
                    CInd = (ImageList[j].MaxParty % (t * 10)) / t;
                    ImageListTemp[C[CInd] - 1] = ImageList[j];
                    C[CInd] = C[CInd] - 1;
                }
                t *= 10;
                ImageList = new List<SubTextureStruct>(ImageListTemp);
            }
        }

        private void Arrangement(ref List<SubTextureStruct> ImageList)
        {
            List<System.Drawing.Rectangle> TakePlaceList = new List<System.Drawing.Rectangle>();
            SubTextureStruct SubTexture;
            System.Drawing.Rectangle Rct, TakeRct;
            const int TextureHeight = 4096, TextureWidth = 4096;
            int heighAtlas = 0;
            int CurrentY, j, k;
            TakePlaceList.Add(ImageList[0].LocRect);
            try 
            {
                for (int i = 1; i < ImageList.Count; i++)
                {
                    SubTexture = ImageList[i];
                    Rct = SubTexture.LocRect;
                    Rct.Width = Rct.Width + 1;
                    CurrentY = TextureHeight;
                    j = 0;
                    while (j < TakePlaceList.Count)
                        if (TakePlaceList[j].IntersectsWith(Rct))
                        {
                            Rct.Location = new System.Drawing.Point(TakePlaceList[j].Right + 1, Rct.Y);
                            if (TakePlaceList[j].Bottom > Rct.Y)
                                CurrentY = Math.Min(CurrentY, TakePlaceList[j].Bottom - Rct.Y + 1);
                            if (Rct.Right > TextureWidth)
                            {
                                Rct.Location = new System.Drawing.Point(0, Rct.Y + CurrentY);
                                CurrentY = TextureHeight;
                            }
                            j = TakePlaceList.Count - 1;
                            while ((j > 0) && (TakePlaceList[j].Bottom > Rct.Y))
                                j--;
                        }
                        else
                            j++;
                    if (Rct.Bottom > TextureHeight)
                    {
                        throw new Exception("Невозможно разместить изображения в атласе. Попробуйте уменьшить размер изображений или количество.");
                    }
                    j = TakePlaceList.Count - 1;
                    while ((j >= 0) && (TakePlaceList[j].Bottom > Rct.Bottom))
                        j--;
                    k = j;
                    while ((k >= 0) && (TakePlaceList[k].Bottom == Rct.Bottom))
                        if ((Rct.X == TakePlaceList[k].Right + 1) && (Rct.Y == TakePlaceList[k].Y))
                        {
                            TakeRct = TakePlaceList[k];
                            TakeRct.Width += Rct.Width + 1;
                            TakePlaceList[k] = TakeRct;
                            k = -1;
                            j = -2;
                        }
                        else
                            k--;
                    if (j > -2)
                    {
                        j++;
                        TakePlaceList.Insert(j, Rct);
                    }
                    SubTexture.LocRect = Rct;
                    ImageList[i] = SubTexture;
                    if (heighAtlas < Rct.Bottom)
                    {
                        heighAtlas = Rct.Bottom;
                    }
                }

                HeighAtlas = (heighAtlas % 4 != 0) ? ((heighAtlas / 4 + 1) * 4) : heighAtlas;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateImageList(ref List<SubTextureStruct> ImageList, WotContext context, SaveType atlasType)
        {
            int X, Y, Width, Height, i;
            int BeginCount = ImageList.Count;

            var nameAtlas = atlasType == SaveType.BattleAtlas ? battleAtlas : vehicleMarkerAtlas;

            var StreamAtlasDDS =
                ZipCache.GetZipFileStream(new CompositePath(context, context.Installation.Path,
                    context.VersionConfig.PathSourceAtlas, nameAtlas + ".dds"));

            System.Drawing.Bitmap AtlasPNG = null;
            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    using (MagickImage AtlasDDS = new MagickImage(StreamAtlasDDS))
                    {
                        AtlasDDS.Format = MagickFormat.Png;
                        AtlasDDS.Write(memStream);
                        AtlasPNG = new System.Drawing.Bitmap(memStream);
                    }
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "CreateImageList", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            AtlasPNG.SetResolution(96.0F, 96.0F);
            
            var StreamAtlasXML =
                ZipCache.GetZipFileStream(new CompositePath(context, context.Installation.Path,
                    context.VersionConfig.PathSourceAtlas, nameAtlas + ".xml"));
            XDocument AtlasXML = XDocument.Load(StreamAtlasXML);

            XElement Root = AtlasXML.Element("root");
            SubTextureStruct SubTextureTemp = new SubTextureStruct();
            foreach (XElement element in Root.Elements())
            {
                SubTextureTemp.FName = element.Element("name").Value.Trim();
                i = 0;
                while ((i < BeginCount) && (SubTextureTemp.FName != ImageList[i].FName))
                    i++;
                if (i >= BeginCount)
                {
                    X = Convert.ToInt32(element.Element("x").Value.Trim());
                    Y = Convert.ToInt32(element.Element("y").Value.Trim());
                    Width = Convert.ToInt32(element.Element("width").Value.Trim());
                    Height = Convert.ToInt32(element.Element("height").Value.Trim());
                    SubTextureTemp.ImageTank = new System.Drawing.Bitmap(Width, Height);
                    SubTextureTemp.ImageTank.SetResolution(96.0F, 96.0F);
                    SubTextureTemp.MaxParty = Math.Max(Width, Height);
                    using (System.Drawing.Graphics gPNG = System.Drawing.Graphics.FromImage(SubTextureTemp.ImageTank))
                    {
                        gPNG.DrawImage(AtlasPNG, 0, 0, new System.Drawing.Rectangle(X, Y, Width, Height), System.Drawing.GraphicsUnit.Pixel);
                    }
                    SubTextureTemp.LocRect = new System.Drawing.Rectangle(0, 0, Width, Height);
                    ImageList.Add(SubTextureTemp);
                }
            }
        }

        public void SaveAtlas(string path, SaveType atlasType, IEnumerable<RenderTask> renderTasks)
        {
            List<SubTextureStruct> ImageList = new List<SubTextureStruct>();
            SubTextureStruct SubTexture = new SubTextureStruct();

            foreach (var render in renderTasks)
            {
                SubTexture.FName = render.TankId;
                using (MemoryStream outStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(render.Image));
                    encoder.Save(outStream);
                    SubTexture.ImageTank = new System.Drawing.Bitmap(outStream);
                }
                SubTexture.LocRect = new System.Drawing.Rectangle(0, 0, render.Image.PixelWidth,
                    render.Image.PixelHeight);
                if (atlasType != SaveType.CustomAtlas)
                {
                    SubTexture.MaxParty = Math.Max(render.Image.PixelWidth, render.Image.PixelHeight);
                }
                ImageList.Add(SubTexture);
            }

            if (atlasType != SaveType.CustomAtlas)
            {
                this.CreateImageList(ref ImageList, context, atlasType);
                this.RadixSort(ref ImageList);
            }

            this.Arrangement(ref ImageList);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            this.CreateAtlasImage(ref ImageList, path);
            this.CreateAtlasXML(ref ImageList, path.Replace(".png", ".xml"));
        }
    }
}
