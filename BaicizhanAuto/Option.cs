using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaicizhanAuto
{
    public class Option : Area
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        public Option(int Left, int Top, int Right, int Bottom, string Id, OCREngine OcrEngine) : base(Left, Top, Right, Bottom, Id, OcrEngine)
        {
            ProcessTask = new Task(() => {SaveImgToMem(); Img2String(); });
        }

        public void Click()
        {
            Console.WriteLine($"choosed {Id}. ratio is {Ratio}");
            SetCursorPos((Left + Right) / 2, (Top + Bottom) / 2);
            Thread.Sleep(30);
            mouse_event(0x02 | 0x04, 0, 0, 0, 0); 
            Thread.Sleep(10);
            mouse_event(0x02 | 0x04, 0, 0, 0, 0); 
            Thread.Sleep(30);
            SetCursorPos(0, 0);
        }

        public new void Img2String()
        {
            Text = OcrEngine.ChineseOCRLite(Img).Trim();
        }

        public new Task Process()
        {
            ProcessTask = new Task(() => { SaveImgToMem(); Img2String(); });
            ProcessTask.Start();
            return ProcessTask;
        }

        public new void SaveImgToMem()
        {
            Img = new byte[(Right - Left) * (Bottom - Top) * 4 + 20];
            var scrarea = new Rectangle(Left, Bottom, Right - Left, Bottom - Top);
            var bmp = new Bitmap(scrarea.Width, scrarea.Height);
            var g = Graphics.FromImage(bmp);
            var stream = new MemoryStream(Img, true);

            g.CopyFromScreen(Left, Top, 0, 0, new Size(scrarea.Width, scrarea.Height));
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            bmp.Save($"{Id}.png");
            stream.Close();
        }

        public void CalcRatio(string trans)
        {
            var word_array = Regex.Split(Text, @"\ |\?|\.|\/|\;|\:|′|`|,|，|\"" |\'|·|\r|\n|的|地");
            var total_ratio = 0;
            var count = word_array.Length;
            foreach (var item in word_array)
            {
                if (item != string.Empty)
                {
                    var ratio = FuzzySharp.Fuzz.PartialRatio(item, trans);
                    if (100 == ratio)
                    {
                        Ratio = 100.0;
                        return;
                    }
                    total_ratio += ratio;
                }
                else
                {
                    count -= 1;
                }
            }
            if (0 == count)
            {
                Ratio = 0;
                return;
            }
            else
            {
                Ratio = total_ratio / count;
            }
        }
    }
}
