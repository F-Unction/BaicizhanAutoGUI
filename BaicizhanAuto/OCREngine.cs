using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Ocl;
using Emgu.CV.OCR;
using OcrLiteLib;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace BaicizhanAuto
{
    public class OCREngine
    {
        private OcrLite ChineseOcrLiteEngine;
        private TesseractEngine TesseractEngine;

        public OCREngine(string detPath, string clsPath, string recPath, string keysPath, int numThread, string TesseractPath)
        {
            TesseractEngine = new TesseractEngine(TesseractPath, "eng", EngineMode.Default);
            ChineseOcrLiteEngine = new OcrLite();
            ChineseOcrLiteEngine.InitModels(detPath, clsPath, recPath, keysPath, numThread);
        }
        public string ChineseOCRLite(byte[] Img)
        {
            try
            {
                int padding = 50;
                int imgResize = 1024;
                float boxScoreThresh = (float)0.618;
                float boxThresh = (float)0.300;
                float unClipRatio = (float)2.0;
                bool doAngle = false;
                bool mostAngle = false;
                OcrResult ocrResult = ChineseOcrLiteEngine.Detect(Img, padding, imgResize, boxScoreThresh, boxThresh, unClipRatio, doAngle, mostAngle);
                return ocrResult.StrRes;
            }
            catch (Exception)
            {
                return "";
            }
        }
        public string PaddleOcr(byte[] Img)
        {
            //var  APP_ID = "";
            var API_KEY = "";
            var SECRET_KEY = "";
            var client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
            var img_res = client.GeneralBasic(Img, new Dictionary<string, object>
                {
                    {"language_type", "ENG"},
                    {"detect_direction", "false"},
                    {"detect_language", "false"},
                    {"probability", "false"}
                });
            if (img_res.Property("words_result_num") != null && (int)img_res.Property("words_result_num") != 0)
            {
                img_res.GetValue("words_result");
                return (img_res["words_result"][0]["words"]).ToString().Trim();
            }
            return "";
        }
        public string TesseractOCR(byte[] Img)
        {
            var img = Tesseract.Pix.LoadFromMemory(Img);
            var page = TesseractEngine.Process(img, Tesseract.PageSegMode.SingleWord);
            var str = page.GetText();
            page.Dispose();
            return str;
        }
    }
}
