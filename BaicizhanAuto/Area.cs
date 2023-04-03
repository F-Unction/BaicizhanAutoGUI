using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaicizhanAuto
{

    public class Area
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public string Id;
        public double Ratio;
        public string Text = String.Empty;
        public byte[] Img;
        public OCREngine OcrEngine;

       /*
        public CancellationTokenSource cts= new CancellationTokenSource();
        public CancellationToken CancelToken = CancellationTokenSource.
       */
        public Task? ProcessTask;

        public Area(int Left, int Top, int Right, int Bottom, string Id, OCREngine OcrEngine)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
            this.Id = Id;
            this.OcrEngine = OcrEngine;
        }


        public void SaveImgToMem()
        {
            throw new NotImplementedException();
        }

        public void Img2String()
        {
            throw new NotImplementedException();
        }

        public Task Process()
        {
            throw new NotImplementedException();
        }
    }
}