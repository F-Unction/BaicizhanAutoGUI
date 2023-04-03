using Emgu.CV.Structure;
using SQLite;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
//using Tesseract;

namespace BaicizhanAuto
{
    public class Word : Area
    {
        public string TransResult = String.Empty;
        public SQLiteConnection Database;
        private bool HasWord = false;

        public Word(int Left, int Top, int Right, int Bottom, string Id, OCREngine OCREngine, SQLiteConnection Database) : base(Left, Top, Right, Bottom, Id, OCREngine)
        {
            this.Database = Database;
        }
        public new Task Process()
        {
            ProcessTask = new Task(() =>
            {
                HasWord = false;
                SaveImgToMem();
                if (HasWord)
                {
                    Img2String();
                }
            });
            ProcessTask.Start();
            return ProcessTask;
        }

        static Bitmap Grayscale(Bitmap image)
        {
            Bitmap grayImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int grayValue = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    grayImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            return grayImage;
        }

        static Bitmap Binarize(Bitmap image)
        {
            Bitmap binaryImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int binaryValue = (int)(pixel.R + pixel.G + pixel.B) / 3 > 128 ? 255 : 0;
                    binaryImage.SetPixel(x, y, Color.FromArgb(binaryValue, binaryValue, binaryValue));
                }
            }

            return binaryImage;
        }

        static Bitmap Invert(Bitmap image)
        {
            Bitmap invertedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int invertedValue = 255 - pixel.R;
                    invertedImage.SetPixel(x, y, Color.FromArgb(invertedValue, invertedValue, invertedValue));
                }
            }

            return invertedImage;
        }

        public new void SaveImgToMem()
        {
            int startX = Left;
            int startY = Top;
            int width = Right - Left;
            int height = Bottom - Top;

            Bitmap screenCapture = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(screenCapture))
            {
                g.CopyFromScreen(startX, startY, 0, 0, screenCapture.Size, CopyPixelOperation.SourceCopy);
            }
            screenCapture.Save($"{Id}.png");


            var count = 0;

            for (int x = 0; x < width / 2; x++)
            {
                if (screenCapture.GetPixel(width / 2 + x, (height) / 2).R == 255)
                {
                    count++;
                }
                if (screenCapture.GetPixel(width / 2 - x, (height) / 2).R == 255)
                {
                    count++;
                }
                if (count > 5)
                {
                    HasWord = true;
                    break;
                }
            }
            if (HasWord)
            {
                //screenCapture = Invert(Binarize(Grayscale(screenCapture)));

                //invertedImage.Save($"{Id}.png");
                Img = new byte[width * height * 4];
                MemoryStream ms = new MemoryStream(Img, true);
                //screenCapture.Save($"{Id}.png");
                screenCapture.Save(ms, ImageFormat.Png);
                ms.Close();
            }
            else
            {
                Img = null;
                GC.Collect();
            }
        }

        public new void Img2String()
        {
            Text = OcrEngine.PaddleOcr(Img).Trim();
        }

        public void TransWord()
        {
            // Stardict? No, no, no

            string trans = Database.ExecuteScalar<string>("select translation from stardict where word= ? ", Text);
            string res = String.Empty;
            if (trans != null)
            {
                var matches = Regex.Matches(trans, @"([\u4e00-\u9fff]+)");
                foreach (Match i in matches)
                {
                    res += i.Value + " ";
                }
            }
            TransResult = res;

            /*
            if(File.Exists($"E:\\Projects\\Other\\baicizhan-word-meaning-API\\data\\words\\{Text}.json"))
            {
                string jsonfile = $"E:\\Projects\\Other\\baicizhan-word-meaning-API\\data\\words\\{Text}.json";

                using (System.IO.StreamReader file = System.IO.File.OpenText(jsonfile))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);
                        var value = o["mean_cn"]?.ToString();
                        TransResult = value ?? "";
                        return;
                    }
                }
            }
            TransResult = "";
            */
        }
    }
}
