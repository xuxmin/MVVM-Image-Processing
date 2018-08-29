using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyEdgeDetection
{
    /// <summary>
    /// 将数据锁定在内存中，处理更快
    /// </summary>
    class LockBitmap
    {      
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;
        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                System.Runtime.InteropServices.Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                System.Runtime.InteropServices.Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public System.Drawing.Color GetPixel(int x, int y)
        {
            System.Drawing.Color clr = System.Drawing.Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = System.Drawing.Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = System.Drawing.Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = System.Drawing.Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, System.Drawing.Color color)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
    }
    public static class Canny
    {
        #region Field

        static Bitmap _image;  //输入的图像
        static int _width;      //图像的宽度
        static int _height;     //图像的高度

        static int[,] _grayImage;  //输入的图像灰度化后灰度值存储在这里
        static int[,] _gradient;
        static double[,] _angle;
        
        static float[,] GAUSSIAN_KERNEL = new float[5, 5] { { 1f / 273f, 4f / 273f, 7f / 273f, 4f / 273f, 1f / 273f },
                                                    { 4f / 273f, 16f / 273f , 26f / 273f , 16f / 273f , 4f / 273f },
                                                    { 7f / 273f, 26f / 273f , 41f / 273f , 26f / 273f , 7f / 273f},
                                                    { 4f / 273f, 16f / 273f , 26f / 273f , 16f / 273f , 4f / 273f },
                                                    { 1f / 273f, 4f / 273f, 7f / 273f, 4f / 273f, 1f / 273f }, };


        #endregion

        #region Public Methods

        /// <summary>
        /// Canny检测
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Edges"></param>
        /// <param name="KernelSize"></param>
        /// <param name="Sigma"></param>
        /// <param name="ThrHigh"></param>
        /// <param name="ThrLow"></param>
        public static void DetectCannyEdges(Bitmap Input,out Bitmap Edges,
                                        int KernelSize, float Sigma,
                                        int ThrHigh, int ThrLow)
        {
            ReadImage(Input);

            GenerateGaussianKernel(KernelSize, Sigma, out float[,] GaussianKernel);
            //高斯模糊处理
            _grayImage = ConvolutionFilter2D(_grayImage, _width, _height, GaussianKernel, KernelSize);

            //利用算子获得梯度与梯度方向数组
            Soble(_grayImage, _width, _height, out _gradient, out _angle);

            //非极大值抑制
            Suppression(_image.Width, _height, _gradient, _angle);

            int[,] EdgesMap = new int[_width, _height];

            //双阈值检测与滞后处理
            HysterisisThresholding(_gradient, _width, _height, ThrHigh, ThrLow, ref EdgesMap);

            //获得处理后的图像
            Edges = new Bitmap(_width, _height);
            LockBitmap lockBitmap = new LockBitmap(Edges);
            lockBitmap.LockBits();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (EdgesMap[i, j] == 1)
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.White);
                    }
                    else
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.Black);
                    }

                }
            }
            lockBitmap.UnlockBits();
        }

        /// <summary>
        /// 获得高斯过滤后的图像
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Edges"></param>
        /// <param name="KernelSize"></param>
        /// <param name="Sigma"></param>
        public static void GaussianFilter(Bitmap Input, out Bitmap Edges, int KernelSize, float Sigma)
        {
            ReadImage(Input);

            GenerateGaussianKernel(KernelSize, Sigma, out float[,] GaussianKernel);
            //高斯模糊处理
            _grayImage = ConvolutionFilter2D(_grayImage, _width, _height, GaussianKernel, KernelSize);

            Edges = new Bitmap(_width, _height);
            LockBitmap lockBitmap = new LockBitmap(Edges);
            lockBitmap.LockBits();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    lockBitmap.SetPixel(i, j,Color.FromArgb(_grayImage[i,j], _grayImage[i, j], _grayImage[i, j]));
                }
            }
            lockBitmap.UnlockBits();
        }

        /// <summary>
        /// 获得非极大值抑制后的图像
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Edges"></param>
        /// <param name="KernelSize"></param>
        /// <param name="Sigma"></param>
        public static void Suppression(Bitmap Input, out Bitmap Edges,int KernelSize, float Sigma)
        {
            ReadImage(Input);

            GenerateGaussianKernel(KernelSize, Sigma, out float[,] GaussianKernel);
            //高斯模糊处理
            _grayImage = ConvolutionFilter2D(_grayImage, _width, _height, GaussianKernel, KernelSize);

            //利用算子获得梯度与梯度方向数组
            Soble(_grayImage, _width, _height, out _gradient, out _angle);

            //非极大值抑制
            Suppression(_image.Width, _height, _gradient, _angle);

            Edges = new Bitmap(_width, _height);
            LockBitmap lockBitmap = new LockBitmap(Edges);
            lockBitmap.LockBits();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (_gradient[i, j] > 255) _gradient[i, j] = 255;
                    else if (_gradient[i, j] < 0) _gradient[i, j] = 0;
                    lockBitmap.SetPixel(i, j, Color.FromArgb(_gradient[i, j], _gradient[i, j], _gradient[i, j]));
                }
            }
            lockBitmap.UnlockBits();
        }

        /// <summary>
        /// 获得强弱边缘图像
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="SEIamge"></param>
        /// <param name="WEImage"></param>
        /// <param name="KernelSize"></param>
        /// <param name="Sigma"></param>
        /// <param name="ThrHigh"></param>
        /// <param name="ThrLow"></param>
        public static void Threshold(Bitmap Input, 
                                        out Bitmap SEIamge, out Bitmap WEImage,
                                        int KernelSize, float Sigma,
                                        int ThrHigh, int ThrLow)
        {
            ReadImage(Input);

            GenerateGaussianKernel(KernelSize, Sigma, out float[,] GaussianKernel);
            //高斯模糊处理
            _grayImage = ConvolutionFilter2D(_grayImage, _width, _height, GaussianKernel, KernelSize);

            //利用算子获得梯度与梯度方向数组
            Soble(_grayImage, _width, _height, out _gradient, out _angle);

            //非极大值抑制
            Suppression(_image.Width, _height, _gradient, _angle);

            int[,] EdgesMap = new int[_width, _height];

            //双阈值检测与滞后处理
            Thresholding(_gradient, _width, _height, ThrHigh, ThrLow, out EdgesMap);
            
            //获得处理后的图像
            SEIamge = new Bitmap(_width, _height);
            WEImage = new Bitmap(_width, _height);

            LockBitmap lockBitmap = new LockBitmap(SEIamge);
            lockBitmap.LockBits();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (EdgesMap[i, j] == 2)
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.White);
                    }
                    else
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.Black);
                    }

                }
            }
            lockBitmap.UnlockBits();

            lockBitmap = new LockBitmap(WEImage);
            lockBitmap.LockBits();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (EdgesMap[i, j] == 1 )
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.White);
                    }
                    else
                    {
                        lockBitmap.SetPixel(i, j, System.Drawing.Color.Black);
                    }

                }
            }
            lockBitmap.UnlockBits();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// 将灰度化的图像储存于grayImage中
        /// </summary>
        static void ReadImage(Bitmap image)
        {
            _image = image;
            _width = image.Width;
            _height = image.Height;
            _grayImage = new int[image.Width, image.Height];
            LockBitmap lockBitmap = new LockBitmap(image);
            lockBitmap.LockBits();
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    _grayImage[i, j] = (int)(0.3 * lockBitmap.GetPixel(i, j).R + 0.59 * lockBitmap.GetPixel(i, j).G + 0.11 * lockBitmap.GetPixel(i, j).B);
                }
            }
            lockBitmap.UnlockBits();
        }


        /*
         * Specific Kernel Size (N)
         * the selection of the size of the Gaussian kernel will affect the performance of the detector
         * The larger the size is, the lower the detector’s sensitivity to noise.Additionally,
         * he localization error to detect the edge will slightly increase with the increase of the Gaussian filter kernel size.
         * A 5×5 is a good size for most cases, but this will also vary depending on specific situations.
         */
        /// <summary>
        /// 生成指定大小的高斯卷积核
        /// </summary>
        /// <param name="N">N为卷积核的大小，常为3，5，7，越大对噪声的敏感越弱， </param>
        /// <param name="S">S为sigma的大小，</param>
        /// <param name="GaussianKernel"></param>       
        static void GenerateGaussianKernel(int N, float S, out float[,] GaussianKernel)
        {

            float Sigma = S;
            float pi;
            pi = (float)Math.PI;
            int i, j;
            int SizeofKernel = N;

            float[,] Kernel = new float[N, N];
            float sum = 0;

            float D1, D2;
            D1 = 1 / (2 * pi * Sigma * Sigma);
            D2 = 2 * Sigma * Sigma;

            for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
            {
                for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                {
                    Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (D1 * (float)Math.Exp(-(i * i + j * j) / D2));   //这里是不是写错了，（1/D1）还是（D1）
                    sum += Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                }
            }
            for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
            {
                for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                {
                    Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] / sum;
                }
            }
            GaussianKernel = Kernel;

        }

        /// <summary>
        /// 对图像进行卷积操作
        /// </summary>
        /// <param name="Data">图像的灰度值</param>
        /// <param name="Width">图像的宽度</param>
        /// <param name="Height">图像的高度</param>
        /// <param name="Kernel">卷积核</param>
        /// <param name="KernelSize">卷积核的大小</param>
        /// <returns>返回卷积后图像数据</returns>
        static int[,] ConvolutionFilter2D(int[,] Data, int Width, int Height, float[,] Kernel, int KernelSize)
        {
            int i, j, k, l;
            int Limit = KernelSize / 2;

            int[,] output = new int[Width, Height];

            float Sum = 0;

            for (i = Limit; i <= ((Width - 1) - Limit); i++)
            {
                for (j = Limit; j <= ((Height - 1) - Limit); j++)
                {
                    Sum = 0;
                    for (k = -Limit; k <= Limit; k++)
                    {

                        for (l = -Limit; l <= Limit; l++)
                        {
                            Sum = Sum + ((float)Data[i + k, j + l] * Kernel[Limit + k, Limit + l]);
                        }
                    }
                    if ((int)(Math.Round(Sum)) < 0)
                    {
                        output[i, j] = 0;
                    }
                    else if ((int)(Math.Round(Sum)) > 255)
                    {
                        output[i, j] = 255;
                    }
                    else
                    {
                        output[i, j] = (int)(Math.Round(Sum));
                    }

                }
            }
            return output;
        }

        /// <summary>
        /// 根据图像计算梯度值和方向
        /// </summary>
        /// <param name="Data">图像的灰度数据</param>
        /// <param name="Width">图像的宽度</param>
        /// <param name="Height">图像的高度</param>
        /// <param name="Gradient">图像的梯度值</param>
        /// <param name="Angle">图像梯度值的方向，Angle 的范围在（-180°，180°]</param>
        static void Soble(int[,] Data, int Width, int Height, out int[,] Gradient, out double[,] Angle)
        {
            //double[,] P = new double[Width, Height];
            //double[,] Q = new double[Width, Height]; 

            int[,] P = new int[Width, Height];
            int[,] Q = new int[Width, Height];
            float[,] Gx = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            float[,] Gy = new float[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            P = ConvolutionFilter2D(Data, Width, Height, Gx, 3);
            Q = ConvolutionFilter2D(Data, Width, Height, Gy, 3);

            Gradient = new int[Width, Height];
            Angle = new double[Width, Height];

            for (int i = 0; i < Width - 1; i++)
            {
                for (int j = 0; j < Height - 1; j++)
                {
                    //防止数组越界
                    //P[i, j] = (double)(Data[i + 1, j] - Data[i, j] + Data[i + 1, j + 1] - Data[i, j + 1]) / 2;
                    //Q[i, j] = (double)(Data[i, j] - Data[i, j + 1] + Data[i + 1, j] - Data[i + 1, j + 1]) / 2;
                    //Gradient[i, j] = (int)(Math.Sqrt(P[i, j] * P[i, j] + Q[i, j] * Q[i, j]));
                    Gradient[i, j] = Math.Abs(P[i, j]) + Math.Abs(Q[i, j]);  //这样更快
                    Angle[i, j] = Math.Atan2(Q[i, j], P[i, j]) * 180 / 3.1415;     //atan2 的范围是(-Π，Π]
                }
            }
        }

        /*
         * Non-maximum suppression
         * 除了 the local maxima，supress其他所有的梯度值（将他们设置成0）
         * 算法：对于每一个像素，将它与其梯度方向及其反方向比较，如果是最大的话，就保留该值，如果不是就suppress为0
         * 在具体实现中，将方向具体分成4个，0，45，90，-45，然后将这个像素与方向上相邻的两个比较，是最大的话，就保留，不是变为0
         */
        /// <summary>
        /// 对梯度值进行非最大值抑制
        /// </summary>
        /// <param name="Data">图像灰度数组</param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Gradient">图像的梯度</param>
        /// <param name="Angle">图像的梯度方向</param>
        static void Suppression(int Width, int Height, int[,] Gradient, double[,] Angle)
        {
            for (int i = 1; i < Width - 1; i++)
            {
                for (int j = 1; j < Height - 1; j++)
                {

                    //Horizontal angle, vertical edge (-22.5~22.5)
                    if (Math.Abs(Angle[i, j]) <= 22.5 || Math.Abs(Angle[i, j]) >= 157.5)
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j]) || (Gradient[i, j] < Gradient[i - 1, j]))
                        {
                            Gradient[i, j] = 0;
                        }

                    }
                    //Vertical angle, horizontal edge (67.5~90 or -67.5~-90)
                    else if (Math.Abs(Angle[i, j]) > 67.5 && Math.Abs(Angle[i, j]) < 112.5)
                    {
                        if ((Gradient[i, j] < Gradient[i, j + 1]) || (Gradient[i, j] < Gradient[i, j - 1]))
                        {
                            Gradient[i, j] = 0;
                        }
                    }

                    //-45 degree angle, +45 Degree Edge
                    else if ((-67.5 < Angle[i, j]) && (Angle[i, j] <= -22.5) || Angle[i, j] > 112.5 && Angle[i, j] < 157.5)
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j + 1]) || (Gradient[i, j] < Gradient[i - 1, j - 1]))
                        {
                            Gradient[i, j] = 0;
                        }
                    }

                    //45 degree angle, -45 degree Edge
                    else if ((22.5 < Angle[i, j]) && (Angle[i, j] <= 67.5) || (Angle[i, j] > -157.5) && (Angle[i, j] <= -112.5))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j - 1]) || (Gradient[i, j] < Gradient[i - 1, j + 1]))
                        {
                            Gradient[i, j] = 0;
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 根据梯度值与双阈值获得强弱边缘图像
        /// </summary>
        /// <param name="Gradient"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="ThrHigh"></param>
        /// <param name="ThrLow"></param>
        /// <param name="edge">2为强边缘1为弱边缘0为被抑制</param>
        static void Thresholding(int[,] Gradient, int Width, int Height, int ThrHigh, int ThrLow, out int[,] edge)
        {
            edge = new int[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Gradient[i, j] >= ThrHigh)
                    {
                        edge[i, j] = 2;
                    }
                    else if (Gradient[i, j] >= ThrLow && Gradient[i, j] < ThrHigh)
                    {
                        edge[i, j] = 1;
                    }
                    else
                    {
                        edge[i, j] = 0;
                    }
                }
            }
        }


        /*
        * Edge tracking by hysteresis
        * the strong edge pixels should certainly be involved in the final edge image
        * there will be some debate on the weak edge pixels, as these pixels can either be extracted from the true edge, or the noise/color variations.
        * Usually a weak edge pixel caused from true edges will be connected to a strong edge pixel while noise responses are unconnected.
        * To track the edge connection, blob analysis is applied by looking at a weak edge pixel and its 8-connected neighborhood pixels. 
        * As long as there is one strong edge pixel that is involved in the blob, that weak edge point can be identified as one that should be preserved.
        * 强边缘像素要保留，弱边缘像素如果相邻的像素有强边缘的，那就保留，否则不保留
        * */
        /// <summary>
        /// Edge tracking by hysteresis
        /// </summary>
        /// <param name="Edges"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="EdgeMap">生成的正式的图像，1为边缘，0为没有</param>

        static void HysterisisThresholding(int[,] Gradient, int Width, int Height, int ThrHigh, int ThrLow, ref int[,] EdgeMap)
        {
            int[,] EdgePoints = new int[Width, Height];

            for (int i = 0; i < Width - 1; i++)
            {
                for (int j = 0; j < Height - 1; j++)
                {
                    if (Gradient[i, j] >= ThrHigh)
                    {
                        EdgePoints[i, j] = 1;
                    }
                    else if (Gradient[i, j] >= ThrLow && Gradient[i, j] < ThrHigh)
                    {
                        EdgePoints[i, j] = 2;
                    }
                }
            }
            int Limit = 1;
            int[,] VisitedMap = new int[Width, Height];

            for (int i = Limit; i <= (Width - 1) - Limit; i++)
                for (int j = Limit; j <= (Height - 1) - Limit; j++)
                {
                    if (EdgePoints[i, j] == 1)
                    {
                        EdgeMap[i, j] = 1;

                    }

                }

            for (int i = Limit; i <= (Width - 1) - Limit; i++)
            {
                for (int j = Limit; j <= (Height - 1) - Limit; j++)
                {
                    if (EdgePoints[i, j] == 1)
                    {
                        EdgeMap[i, j] = 1;
                        Travers(i, j, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                        VisitedMap[i, j] = 1;
                    }
                }
            }

            return;
        }

        static void Travers(int X, int Y, ref int[,] VisitedMap, ref int[,] EdgePoints, ref int[,] EdgeMap)
        {


            if (VisitedMap[X, Y] == 1)
            {
                return;
            }

            //1
            if (EdgePoints[X + 1, Y] == 2)
            {
                EdgeMap[X + 1, Y] = 1;
                VisitedMap[X + 1, Y] = 1;
                Travers(X + 1, Y, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }
            //2
            if (EdgePoints[X + 1, Y - 1] == 2)
            {
                EdgeMap[X + 1, Y - 1] = 1;
                VisitedMap[X + 1, Y - 1] = 1;
                Travers(X + 1, Y - 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }

            //3

            if (EdgePoints[X, Y - 1] == 2)
            {
                EdgeMap[X, Y - 1] = 1;
                VisitedMap[X, Y - 1] = 1;
                Travers(X, Y - 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }

            //4

            if (EdgePoints[X - 1, Y - 1] == 2)
            {
                EdgeMap[X - 1, Y - 1] = 1;
                VisitedMap[X - 1, Y - 1] = 1;
                Travers(X - 1, Y - 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }
            //5
            if (EdgePoints[X - 1, Y] == 2)
            {
                EdgeMap[X - 1, Y] = 1;
                VisitedMap[X - 1, Y] = 1;
                Travers(X - 1, Y, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }
            //6
            if (EdgePoints[X - 1, Y + 1] == 2)
            {
                EdgeMap[X - 1, Y + 1] = 1;
                VisitedMap[X - 1, Y + 1] = 1;
                Travers(X - 1, Y + 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }
            //7
            if (EdgePoints[X, Y + 1] == 2)
            {
                EdgeMap[X, Y + 1] = 1;
                VisitedMap[X, Y + 1] = 1;
                Travers(X, Y + 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }
            //8

            if (EdgePoints[X + 1, Y + 1] == 2)
            {
                EdgeMap[X + 1, Y + 1] = 1;
                VisitedMap[X + 1, Y + 1] = 1;
                Travers(X + 1, Y + 1, ref VisitedMap, ref EdgePoints, ref EdgeMap);
                return;
            }


            //VisitedMap[X, Y] = 1;
            return;
        }


        #endregion


    }
}
