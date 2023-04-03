using BaicizhanAuto;
using System.Runtime.InteropServices;

namespace BaicizhanAutoGUI
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        public static BaicizhanAutoEngine baicizhanAutoEngine;

        public static bool Run = true;

        private Task task = new Task(() =>
        {
            AllocConsole();
            baicizhanAutoEngine = new BaicizhanAutoEngine(@"E:\Projects\Other\OcrLiteOnnxCs\OcrLiteOnnxForm\bin\Debug\models\",
            0, 0, 81, 410, 640, 130, 121, 589, 535, 63, 8,
            @"D:\Program Files\baicizhan_auto\stardict.db", @"D:\Program Files\Tesseract-OCR", Console.Out
            );
            while (true)
            {
                if (Run) {  baicizhanAutoEngine.AutoChoose(); }
            }
        });

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Run = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Run = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Run = false;
            task.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FreeConsole();
        }
    }
}