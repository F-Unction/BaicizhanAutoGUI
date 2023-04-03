using System;
using System.Diagnostics;
using System.Drawing;
using SQLite;

namespace BaicizhanAuto
{
    public class BaicizhanAutoEngine
    {
        public OCREngine chineseOCR;

        private int XOffset, YOffset;
        private int WordLeft, WordTop, WordWidth, WordHeight;
        private int OptionLeft, OptionTop, OptionWidth, OptionHeight, OptionSpace;

        private Word W;
        private Option A, B, C, D;

        public SQLiteConnection Database;

        public string TesseractPath;
        private string LastWord = String.Empty;
        public TextWriter consoleWriter;

        public BaicizhanAutoEngine(string ocrmodelpath, int XOffset, int YOffset, int WordLeft, int WordTop, int WordWidth, int WordHeight,
            int OptionLeft, int OptionTop, int OptionWidth, int OptionHeight, int OptionSpace, string DatabasePath, string TesseractPath, TextWriter writer)
        {
            this.chineseOCR = new OCREngine(ocrmodelpath + "dbnet.onnx", ocrmodelpath + "angle_net.onnx", ocrmodelpath + "crnn_lite_lstm.onnx", ocrmodelpath + "keys.txt", 1, TesseractPath);
            this.XOffset = XOffset;
            this.YOffset = YOffset;
            this.WordLeft = WordLeft;
            this.WordTop = WordTop;
            this.WordWidth = WordWidth;
            this.WordHeight = WordHeight;
            this.OptionLeft = OptionLeft;
            this.OptionTop = OptionTop;
            this.OptionWidth = OptionWidth;
            this.OptionHeight = OptionHeight;
            this.OptionSpace = OptionSpace;
            this.Database = new SQLiteConnection(DatabasePath);
            this.W = new Word(WordLeft + XOffset, WordTop + YOffset, WordLeft + WordWidth + XOffset, WordTop + WordHeight, "",chineseOCR, Database);
            this.A = new Option(OptionLeft + XOffset, OptionTop + 0 * (OptionHeight + OptionSpace) + YOffset, OptionLeft + OptionWidth + XOffset, OptionTop + OptionHeight + 0 * (OptionHeight + OptionSpace) + YOffset, "A", chineseOCR);
            this.B = new Option(OptionLeft + XOffset, OptionTop + 1 * (OptionHeight + OptionSpace) + YOffset, OptionLeft + OptionWidth + XOffset, OptionTop + OptionHeight + 1 * (OptionHeight + OptionSpace) + YOffset, "B", chineseOCR);
            this.C = new Option(OptionLeft + XOffset, OptionTop + 2 * (OptionHeight + OptionSpace) + YOffset, OptionLeft + OptionWidth + XOffset, OptionTop + OptionHeight + 2 * (OptionHeight + OptionSpace) + YOffset, "C", chineseOCR);
            this.D = new Option(OptionLeft + XOffset, OptionTop + 3 * (OptionHeight + OptionSpace) + YOffset, OptionLeft + OptionWidth + XOffset, OptionTop + OptionHeight + 3 * (OptionHeight + OptionSpace) + YOffset, "D", chineseOCR);
            //this.TesseractPath = TesseractPath;
            consoleWriter = writer;
        }

        public void AutoChoose()
        {
            while (true)
            {
                // 先处理着 万一就用到了呢
                /*
                A.Process();
                B.Process();
                C.Process();
                D.Process();
                */
                W.Process().Wait();

                if (LastWord == W.Text) // remain old word, so not process
                {
                    //A.ProcessTask.
                    continue;
                }
                else // new word found!
                {
                    LastWord = W.Text;
                }
                consoleWriter.WriteLine($"{W.Text} found.");

                if (W.Text.Length <= 2)
                {
                    continue;
                }
                consoleWriter.WriteLine($"{W.Text} confirmed.");

                var TransTask = new Task(() => { W.TransWord(); });
                TransTask.Start();

                Task.WaitAll(A.Process(), B.Process(), C.Process(), D.Process(), TransTask);

                if (W.TransResult == "")
                {
                    continue;
                }

                consoleWriter.WriteLine($"{W.TransResult} translated.");
                break;
            }

            if (B.Text.Contains("开始对战"))
            {
                return;
            }

            A.CalcRatio(W.TransResult);
            B.CalcRatio(W.TransResult);
            C.CalcRatio(W.TransResult);
            D.CalcRatio(W.TransResult);

            GetBestOption().Click();

            new Task<int>(() =>
            {
                consoleWriter.WriteLine("--------------------");
                consoleWriter.WriteLine($"{A.Text}  ratio: {A.Ratio}");
                consoleWriter.WriteLine($"{B.Text}  ratio: {B.Ratio}");
                consoleWriter.WriteLine($"{C.Text}  ratio: {C.Ratio}");
                consoleWriter.WriteLine($"{D.Text}  ratio: {D.Ratio}");
                consoleWriter.WriteLine("--------------------");
                return 0;
            }).Start();
            return;
        }

        public Option GetBestOption()
        {
            var best = A;
            if (B.Ratio > best.Ratio)
            {
                best = B;
            }
            if (C.Ratio > best.Ratio)
            {
                best = C;
            }
            if (D.Ratio > best.Ratio)
            {
                best = D;
            }
            if (best.Ratio == 0)
            {
                if (A.Text == "")
                {
                    best = A;
                }
                else if (B.Text == "")
                {
                    best = B;
                }
                else if (C.Text == "")
                {
                    best = C;
                }
                else
                {
                    best = D;
                }
            }

            return best;
        }
    }
}