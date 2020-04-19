using System.Drawing;
using System.Threading.Tasks;
using Tesseract;
using System.IO;

namespace PUBG_MAPHACK
{
    public class ScreenAnalyzer
    {

        public TesseractEngine engine;


        public ScreenAnalyzer() {
            // Copy tessdata to tmp folder
            if (Directory.Exists("x86") && !Directory.Exists(Path.GetTempPath() + @"\tessdata"))
            {
                Directory.CreateDirectory(Path.GetTempPath() + @"\tessdata");

                foreach (string newPath in Directory.GetFiles(@"x86\tessdata", "*.*", SearchOption.AllDirectories))
                {
                    System.Console.WriteLine(newPath);
                    File.Copy(newPath, Path.GetTempPath() + @"\tessdata\" + Path.GetFileName(newPath), true);
                }
                   
            }
        }

        public async Task<string> Analyze(int x, int y, int width, int height)
        {
            string textInImage = "";

            try
            {
                textInImage = await Task.Run(() => {
                    // Let's capture part of screen for analysis
                    Bitmap image = CaptureScreen(x, y, width, height);

                    // Let's get any text inside screenshot
                    return getTextFromImage(image);
                });
            }
            catch
            {
                // YEET
            }

            // Return our results
            return textInImage.ToLower();
        }

        public Bitmap CaptureScreen(int x, int y, int width, int height)
        {
            // Let's create a rectangle (Hodor!)
            Rectangle bounds = new Rectangle(x, y, width, height);

            // Let's create a bitmap same size as our rectangle (Hodor!)
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

            // Let's create a graphics object from the bitmap (Hodor!)
            Graphics g = Graphics.FromImage(bitmap);

            // Let's capture part of our screen in our graphic (Hodor!)
            g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

            /* If we build with debugging mode lets save printscreens in debugging folder */
            if(Program.debug == true)
            {
               bitmap.Save(@"debug\printscreens\screen-" + System.DateTime.Now.ToFileTime() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
           
            return bitmap;
        }

        public string getTextFromImage(Bitmap bitmap)
        {
            string tessdata = "";
            if (Directory.Exists(Path.GetTempPath() + @"tessdata"))
            {
                tessdata = Path.GetTempPath() + @"tessdata";
            } else
            {
                if (Directory.Exists(@"x86\tessdata"))
                {
                    tessdata = @"x86\tessdata";
                }
            }

            if (tessdata == "") {
                Program.triggerTracker += @"tessdata not found | ";
                return "";
            }

            //TesseractEnviornment.CustomSearchPath = Path.GetTempPath() + "pubg_maphack";

            /* Let's load the Tesseract library (used to get text from images) 
             * The tessdata has been trained to find only PUBG's ingame font with characters "0-9/kmh" (to find the speed)
             * If you want to detect other stuff you need to download the original tessdata files or make your own */
            using (TesseractEngine engine = new TesseractEngine(tessdata, "eng", EngineMode.Default))
            {
                // Set segmentation mode to SparseText for best results (tested all)
                engine.DefaultPageSegMode = PageSegMode.SparseText;

                //engine.SetVariable("tessedit_ocr_engine_mode", 1);

                // Convert pixels
                var pix = PixConverter.ToPix(bitmap);

                // Do the magic
                var page = engine.Process(pix);

                return page.GetText();
            }
        }

    }
}
