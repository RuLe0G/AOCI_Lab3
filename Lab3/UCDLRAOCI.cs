using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing.Imaging;
using System.Drawing;

namespace lab3
{
    class UCDLRAOCI
    {

        public double cannyThreshold = 80;
        public double cannyThresholdLinking = 40;
        public byte color_1 = 0;
        public byte color_2 = 25;
        public byte color_3 = 180;
        public byte color_4 = 210;

        public byte Hsv0 = 143;
        public byte Hsv1 = 95;
        public byte Hsv2 = 75;

        public Image<Bgr, byte> sourceImage; //глобальная переменная
        public Image<Bgr, byte> SecondImage; //глобальная переменная
        public Image<Bgr, byte> MainImageExp; //глобальная переменная
        public Image<Bgr, byte> TempImage; //глобальная переменная

        public VideoCapture capture;
        private int framecount = 0;
        private float brightn = 1;
        private float contrastC;

        private List<byte> l = new List<byte>();


        public void Source(string fileName)
        {
            sourceImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
            MainImage();
        }
        public void Source(Image<Bgr, byte> image)
        {
            sourceImage = image;
            MainImage();
        }
        public void Source(Mat image)
        {
            sourceImage = image.ToImage<Bgr, byte>();
            MainImage();
        }

        public void SourceSecond(string fileName)
        {
            SecondImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
        }
        public void SourceSecond(Image<Bgr, byte> img)
        {
            SecondImage = img;
        }

        public void MainImage()
        {
            MainImageExp = sourceImage.Copy();
        }


        public void CaptureSource(string fileName)
        {
            capture = new VideoCapture(fileName);

        }
        public int FPS()
        {
            return (int)Math.Round(capture.GetCaptureProperty(CapProp.Fps));
        }

        //Обработка фото
        public Image<Gray, byte> CannyProcess()
        {
            Image<Gray, byte> grayImage = sourceImage.Convert<Gray, byte>();
            var tempImage = grayImage.PyrDown();
            var destImage = tempImage.PyrUp();
            Image<Gray, byte> cannyEdges = destImage.Canny(cannyThreshold, cannyThresholdLinking);
            return (cannyEdges.Resize(640, 480, Inter.Linear));
        }

        public bool VideoPlay()
        {
            framecount++;
            var frame = capture.QueryFrame();
            Source(frame);
            if (framecount >= capture.GetCaptureProperty(CapProp.FrameCount))
            {

                framecount = 0;
                return false;
            }
            else return true;

        }

        public Image<Bgr, byte> CellShadingProcess()
        {
            Image<Gray, byte> cannyEdges = CannyProcess();
            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();
            var resultImage = sourceImage.Sub(cannyEdgesBgr); // попиксельное вычитание
            //обход по каналам
            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++) // обход по пискелям
                    {
                        // получение цвета пикселя
                        byte color = resultImage.Data[y, x, channel];
                        if (color <= 50)
                            color = color_1;
                        else if (color <= 100)
                            color = color_2;
                        else if (color <= 150)
                            color = color_3;
                        else if (color <= 200)
                            color = color_4;
                        else
                            color = 255;
                        resultImage.Data[y, x, channel] = color; // изменение цвета пикселя
                    }
            return (resultImage.Resize(640, 480, Inter.Linear));
        }

        public Image<Bgr, byte> Chanel(int i)
        {
            var channel = MainImageExp.Split()[i];

            Image<Bgr, byte> destImage = MainImageExp.CopyBlank();

            VectorOfMat vm = new VectorOfMat();

            switch (i)
            {
                case 0:
                    vm.Push(channel);
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel.CopyBlank());
                    break;

                case 1:
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel);
                    vm.Push(channel.CopyBlank());
                    break;
                case 2:
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel);
                    break;
                default:
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel.CopyBlank());
                    vm.Push(channel.CopyBlank());
                    break;
            }

            CvInvoke.Merge(vm, destImage);

            MainImageExp = destImage.Resize(640, 480, Inter.Linear);

            return (MainImageExp);
        }


        public Image<Bgr, byte> Monochrome()
        {
            var grayImage = new Image<Gray, byte>(MainImageExp.Size);

            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    grayImage.Data[y, x, 0] = Convert.ToByte(0.299 * MainImageExp.Data[y, x, 2] + 0.587 * MainImageExp.Data[y, x, 1] + 0.114 * MainImageExp.Data[y, x, 0]);
                }
            }

            MainImageExp = grayImage.Convert<Bgr, byte>();

            return (MainImageExp);
        }

        public Image<Bgr, byte> Sepia()
        {
            Image<Bgr, byte> destImage = MainImageExp.Copy();

            for (int y = 0; y < destImage.Height; y++)
            {
                for (int x = 0; x < destImage.Width; x++)
                {

                    destImage.Data[y, x, 2] = Convert.ToByte(checkByte(0.393 * MainImageExp.Data[y, x, 2] + 0.769 * MainImageExp.Data[y, x, 1] + 0.189 * MainImageExp.Data[y, x, 0]));

                    destImage.Data[y, x, 1] = Convert.ToByte(checkByte(0.349 * MainImageExp.Data[y, x, 2] + 0.686 * MainImageExp.Data[y, x, 1] + 0.168 * MainImageExp.Data[y, x, 0]));

                    destImage.Data[y, x, 0] = Convert.ToByte(checkByte(0.272 * MainImageExp.Data[y, x, 2] + 0.534 * MainImageExp.Data[y, x, 1] + 0.131 * MainImageExp.Data[y, x, 0]));

                }
            }

            MainImageExp = destImage.Resize(640, 480, Inter.Linear);
            return (MainImageExp);
        }

        public Image<Bgr, byte> Brightness_Contrast(float b = 0, float c = 1)
        {
            TempImage = MainImageExp.Copy();


            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = 0; y < TempImage.Height; y++)
                {
                    for (int x = 0; x < TempImage.Width; x++)
                    {
                        TempImage.Data[y, x, ch] = Convert.ToByte(checkByte(TempImage.Data[y, x, ch] * c + b));
                    }
                }
            }
            return (TempImage);
        }

        public void confirm()
        {
            MainImageExp = TempImage;
        }


        public Image<Hsv, byte> ConvertToHsv(int hs1)
        {

            Image<Hsv, byte> hsvImage = MainImageExp.Convert<Hsv, byte>();

            for (int y = 0; y < hsvImage.Height; y++)
            {
                for (int x = 0; x < hsvImage.Width; x++)
                {
                    hsvImage.Data[y, x, 0] = Convert.ToByte(checkByte(hs1));
                }
            }

            return hsvImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Hsv, byte> ConvertToHsv()
        {

            Image<Hsv, byte> hsvImage = MainImageExp.Convert<Hsv, byte>();


            return hsvImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Bgr, byte> Addition(double a = 7, double b = 3)
        {
            Image<Bgr, byte> res = MainImageExp.CopyBlank();
            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = 0; y < MainImageExp.Height; y++)
                {
                    for (int x = 0; x < MainImageExp.Width; x++)
                    {
                        res.Data[y, x, ch] = (byte)AddColors(MainImageExp.Data[y, x, ch], SecondImage.Data[y, x, ch], a / 10, b / 10);
                    }
                }
            }
            MainImageExp = res.Resize(640, 480, Inter.Linear);
            return MainImageExp;
        }


        public Image<Bgr, byte> Subtraction(double a = 0.7, double b = 0.3)
        {
            Image<Bgr, byte> res = MainImageExp.CopyBlank();
            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = 0; y < MainImageExp.Height; y++)
                {
                    for (int x = 0; x < MainImageExp.Width; x++)
                    {
                        res.Data[y, x, ch] = (byte)SubColors(MainImageExp.Data[y, x, ch], SecondImage.Data[y, x, ch], a / 10, b / 10);
                    }
                }
            }
            MainImageExp = res.Resize(640, 480, Inter.Linear);
            return MainImageExp;
        }

        public Image<Bgr, byte> IDK()
        {
            Image<Bgr, byte> res = MainImageExp.CopyBlank();
            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = 0; y < MainImageExp.Height; y++)
                {
                    for (int x = 0; x < MainImageExp.Width; x++)
                    {
                        res.Data[y, x, ch] = (byte)IdkColors(MainImageExp.Data[y, x, ch], SecondImage.Data[y, x, ch]);
                    }
                }
            }
            MainImageExp = res.Resize(640, 480, Inter.Linear);
            return MainImageExp;
        }

        private double AddColors(double color1, double color2, double a, double b)
        {
            return checkByte(color1 * a + color2 * b);
        }

        private double SubColors(double color1, double color2, double a, double b)
        {
            return checkByte(color1 * a - color2 * b);
        }

        private double IdkColors(double color1, double color2)
        {
            return checkByte((int)color1 ^ (int)color2);
        }


        public Image<Bgr, byte> median()
        {

            Image<Bgr, byte> grayImage = MainImageExp.Convert<Bgr, byte>();
            Image<Bgr, byte> res = grayImage.CopyBlank();

            int sh = 1;
            int N = 9;
            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = sh; y < grayImage.Height - sh; y++)
                {
                    for (int x = sh; x < grayImage.Width - sh; x++)
                    {
                        l.Clear();

                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                l.Add(grayImage.Data[i + y, j + x, ch]);
                            }
                        l.Sort();

                        res.Data[y, x, ch] = l[N / 2];

                    }
                }
            }
            MainImageExp = res;
            return MainImageExp;
        }

        public Image<Bgr, byte> Sharp()
        {
            int[,] w = new int[3, 3]
            {
                { -1, -1, -1},
                { -1,  9, -1},
                { -1, -1, -1},
            };
            return MatFlt(w);
        }

        public Image<Bgr, byte> embos()
        {


            int[,] w = new int[3, 3]
            {
                { -4, -2, 0},
                { -2,  1, 2},
                { 0, 2, 4},
            };

            return MatFlt(w);
        }

        public Image<Bgr, byte> Nars()
        {

            int[,] w = new int[3, 3]
            {
                { 0, -2, 0},
                { -2, 4, 0},
                { 0, 0, 0},
            };
            return MatFlt(w);
        }

        public Image<Bgr, byte> MatFlt(int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9)
        {
            int[,] w = new int[3, 3]
            {
                { i1, i2, i3},
                { i4,  i5, i6},
                { i7, i8, i9},
            };
            return MatFlt(w);
        }

        public Image<Bgr, byte> MatFlt(int[,] w)
        {

            Image<Bgr, byte> grayImage = MainImageExp.Convert<Bgr, byte>();
            Image<Bgr, byte> res = grayImage.CopyBlank();

            int sum = sumMat(w);
            for (int ch = 0; ch < 3; ch++)
            {
                for (int y = 1; y < grayImage.Height - 1; y++)
                {
                    for (int x = 1; x < grayImage.Width - 1; x++)
                    {
                        int r = 0;
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                r += (grayImage.Data[i + y, i + x, ch] * w[i + 1, j + 1]);
                            }

                        res.Data[y, x, ch] = (byte)checkByte(r);

                    }
                }
            }
            MainImageExp = res;
            return MainImageExp;
        }

        public Image<Bgr, byte> BinarPrg(int i)
        {
            if (i % 2 == 0)
            {
                i += 1;
            }
            var edges = MainImageExp.Convert<Gray, byte>();
            edges = edges.ThresholdAdaptive(new Gray(100), AdaptiveThresholdType.MeanC,
            ThresholdType.Binary, i, new Gray(i / 100));
            MainImageExp = edges.Convert<Bgr, byte>();
            return MainImageExp;
        }

        public Image<Bgr, byte> MedianFiltr()
        {

            return MainImageExp;
        }

        public Image<Bgr, byte> Scale(double kX, double kY)
        {

            Image<Bgr, byte> scalingImg = new Image<Bgr, byte>((int)(MainImageExp.Width * kX), (int)(MainImageExp.Height * kY));

            for (int i = 0; i < scalingImg.Width - 1; i++)
            {
                for (int j = 0; j < scalingImg.Height - 1; j++)
                {
                    double I = i / kX;
                    double J = j / kY;

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);


                    if (baseI >= MainImageExp.Width - 1) continue;
                    if (baseJ >= MainImageExp.Height - 1) continue;

                    double rI = I - baseI;
                    double rJ = J - baseJ;

                    double irI = 1 - rI;
                    double irJ = 1 - rJ;

                    Bgr c1 = new Bgr
                    {
                        Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 0] * rI,
                        Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 1] * rI,
                        Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 2] * rI
                    };

                    Bgr c2 = new Bgr
                    {
                        Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 0] * rI,
                        Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 1] * rI,
                        Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 2] * rI
                    };

                    Bgr c = new Bgr
                    {
                        Blue = c1.Blue * irJ + c2.Blue * rJ,
                        Green = c1.Green * irJ + c2.Green * rJ,
                        Red = c1.Red * irJ + c2.Red * rJ
                    };

                    scalingImg[j, i] = c;
                }
            }
            MainImageExp = scalingImg;
            return (scalingImg);
        }
        public Image<Bgr, byte> reflection(int xx, int yy)
        {
            var resultImage = sourceImage.CopyBlank();
            int newX, newY;

            for (int x = 0; x < resultImage.Width - 1; x++)
            {
                for (int y = 0; y < resultImage.Height - 1; y++)
                {

                    if (xx == -1)
                    {
                        newX = x * xx + MainImageExp.Width - 1;
                    }
                    else
                    {
                        newX = x;
                    }

                    if (yy == -1)
                    {
                        newY = y * yy + MainImageExp.Height - 1;
                    }
                    else
                    {
                        newY = y;
                    }

                    resultImage[newY, newX] = MainImageExp[y, x];
                }
            }
            MainImageExp = resultImage;
            return (MainImageExp);
        }
        public Image<Bgr, byte> rotate(double k, Point p)
        {
            Image<Bgr, byte> reflectionImg = MainImageExp.CopyBlank();
            k = DegreeToRadian(k);
            for (int i = 0; i < reflectionImg.Width - 1; i++)
            {
                for (int j = 0; j < reflectionImg.Height - 1; j++)
                {
                    double newI = (Math.Cos((k)) * (i - p.X) - Math.Sin((k)) * (j - p.Y) + p.X);
                    double newJ = (Math.Sin((k)) * (i - p.X) + Math.Cos((k)) * (j - p.Y) + p.Y);
                    if (newI >= 0 && newJ >= 0 && newI < reflectionImg.Width && newJ < reflectionImg.Height)
                    {
                        double I = newI;
                        double J = newJ;

                        double baseI = Math.Floor(I);
                        double baseJ = Math.Floor(J);

                        if (baseI >= reflectionImg.Width - 1) continue;
                        if (baseJ >= reflectionImg.Height - 1) continue;

                        double rI = I - baseI;
                        double rJ = J - baseJ;

                        double irI = 1 - rI;
                        double irJ = 1 - rJ;

                        Bgr c1 = new Bgr
                        {
                            Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 0] * rI,
                            Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 1] * rI,
                            Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 2] * rI
                        };

                        Bgr c2 = new Bgr
                        {
                            Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 0] * rI,
                            Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 1] * rI,
                            Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)(baseJ + 1), (int)baseI, 2] * rI
                        };

                        Bgr c = new Bgr
                        {
                            Blue = c1.Blue * irJ + c2.Blue * rJ,
                            Green = c1.Green * irJ + c2.Green * rJ,
                            Red = c1.Red * irJ + c2.Red * rJ
                        };

                        reflectionImg[j, i] = c;

                    }
                }
            }
                    
            
            MainImageExp = reflectionImg;
            return (MainImageExp);
        }

        public Image<Bgr, byte> shearing(double xX, double yY)
        {
            var resultImage = MainImageExp.CopyBlank();

            for (int x = 0; x < resultImage.Width - 1; x++)
            {
                for (int y = 0; y < resultImage.Height - 1; y++)
                {

                    int newX = x + Convert.ToInt32(xX * (MainImageExp.Height - y));
                    int newY = y + Convert.ToInt32(yY * x);

                    if (newX < resultImage.Width && newY < resultImage.Height && newX >= 0 && newY >= 0)
                    {
                        resultImage[newY, newX] = MainImageExp[y, x];
                    }
                }
            }
            MainImageExp = resultImage;
            return (MainImageExp);
        }

        public Image<Bgr, byte> homography(Point[] Input)
        {
            var srcPoints = new PointF[]
            {
                new PointF(Input[0].X, Input[0].Y),
                new PointF(Input[1].X, Input[1].Y),
                new PointF(Input[2].X, Input[2].Y),
                new PointF(Input[3].X, Input[3].Y)
            };
                
            var destPoints = new PointF[]
            {
                // плоскость, на которую осуществляется проекция,
                // задаётся четыремя точками
                new PointF(0, 0),
                new PointF(0, MainImageExp.Height - 1),
                new PointF(MainImageExp.Width - 1, MainImageExp.Height - 1),
                new PointF(MainImageExp.Width - 1, 0)
            };
            var homographyMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, destPoints);
            var destImage = new Image<Bgr, byte>(MainImageExp.Size);
            CvInvoke.WarpPerspective(MainImageExp, destImage, homographyMatrix, destImage.Size);

            MainImageExp = destImage;
            return (MainImageExp);
        }




        //обработка видео

        //загрузка
        public Image<Bgr, byte> loadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("Файлы изображений | *.jpg; *.jpeg; *.jpe; *.jfif; *.png");
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            Image<Bgr, byte> sourceImage = null;
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                return sourceImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear); ;             
            }
            else
                return sourceImage;
        }

        public VideoCapture loadVideo()
        {
            var capture = new VideoCapture();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("Файлы видео | *.mp4; *.webm; *.avi; *.mpg; *.mp2; *.mpeg; *.mov; *.wmv");
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                capture = new VideoCapture(fileName);                
            }
            return capture;
        }


        public void saveJpeg(string path)
        {
            // Encoder parameter for image quality

            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = this.getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            MainImageExp.Save(path);
        }

        private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }
        //Добавочные методы
        public int sumMat(int[,] i)
        {
            int r = 0;
            for (int x = 0; x < Math.Sqrt(i.Length); x++)
            {
                for (int y = 0; y < Math.Sqrt(i.Length); y++)
                {
                    r += i[x, y];
                }
            }
            return r;
        }
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        public int checkByte(int r)
        {
            if (r > 255) r = 255;
            if (r < 0) r = 0;
            return r;
        }
        public float checkByte(float r)
        {
            if (r > 255) r = 255;
            if (r < 0) r = 0;
            return r;
        }
        public double checkByte(double r)
        {
            if (r > 255) r = 255;
            if (r < 0) r = 0;
            return r;
        }
    }
}