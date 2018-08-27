using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ContourAnalysisNS
{
    [Serializable]
    public class Contour
    {
        Complex[] array;    //Vector-contour
        public Rectangle SourceBoundingRect;    //轮廓在一个最小的矩形内

        public Contour(int capacity)
        {
            array = new Complex[capacity];
        }
        
        protected Contour()     //不能被外面的函数调用，给类的内部自己用
        {
        }

        //组成Vector-contour的Elementary Vector的个数
        public int Count    
        {
            get
            {
                return array.Length;
            }
        }

        public Complex this[int i]
        {
            get { return array[i]; }
            set { array[i] = value; }
        }

        //通过轮廓像素点，生成array
        public Contour(IList<Point> points, int startIndex, int count)   //IList只是希望使用到IList<T>接口规定的功能而已
            : this(count)     //构造函数初始化器, 在调用这个构造函数的时候，会先自动调用public Contour(int capacity)这个构造函数
        {
            int minX = points[startIndex].X;
            int minY = points[startIndex].Y;
            int maxX = minX;
            int maxY = minY;
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i++)
            {
                var p1 = points[i];
                var p2 = i == endIndex - 1 ? points[startIndex] : points[i + 1];
                array[i] = new Complex(p2.X - p1.X, -p2.Y + p1.Y);    //为什么不是 new Complex(p2.X - p1.X, p2.Y - p1.Y)？？

                if (p1.X > maxX) maxX = p1.X;
                if (p1.X < minX) minX = p1.X;
                if (p1.Y > maxY) maxY = p1.Y;
                if (p1.Y < minY) minY = p1.Y;
            }

            SourceBoundingRect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);  
        }

        public Contour(IList<Point> points)
            : this(points, 0, points.Count)  
        {
        }

        public Contour Clone()
        {
            Contour result = new Contour();
            result.array = (Complex[])array.Clone();
            return result;
        }

        /// <summary>
        /// Returns R^2 of difference of norms
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public double DiffR2(Contour c)
        {
            double max1 = 0;
            double max2 = 0;
            double sum = 0;
            for (int i = 0; i < Count; i++)
            {
                double v1 = array[i].Norma;   //Elementary Vector的长度
                double v2 = c.array[i].Norma;
                if (v1 > max1) max1 = v1;
                if (v2 > max2) max2 = v2;
                double v = v1 - v2;
                sum += v * v;
            }
            double max = Math.Max(max1, max2);
            return 1 - sum / Count / max / max;
        }

        //这个是Vector-contour的长度,
        public double Norma
        {
            get
            {
                double result = 0;
                foreach (var c in array)
                    result += c.NormaSquare;
                return Math.Sqrt(result);
            }
        }

        /// <summary>
        /// Scalar product
        /// </summary>
        public unsafe Complex Dot(Contour c, int shift)
        {
            var count = Count;
            double sumA = 0;
            double sumB = 0;
            fixed (Complex* ptr1 = &array[0])      //fixed 语句可防止垃圾回收器重新定位可移动的变量
            fixed (Complex* ptr2 = &c.array[shift])   //从下标shift处开始作Scalar product,
            fixed (Complex* ptr22 = &c.array[0])
            fixed (Complex* ptr3 = &c.array[c.Count - 1])
            {
                Complex* p1 = ptr1;
                Complex* p2 = ptr2;
                for (int i = 0; i < count; i++)
                {
                    Complex x1 = *p1;
                    Complex x2 = *p2;
                    sumA += x1.a * x2.a + x1.b * x2.b;
                    sumB += x1.b * x2.a - x1.a * x2.b;

                    p1++;
                    if (p2 == ptr3)   //循环，到达最后一个便回到原点
                        p2 = ptr22;
                    else
                        p2++;
                }
            }
            return new Complex(sumA, sumB);
        }

        /// <summary>
        /// Intercorrelcation function (ICF)
        /// </summary>
        public Contour InterCorrelation(Contour c)
        {
            int count = Count;
            Contour result = new Contour(count);
            for (int i = 0; i < count; i++)
                result.array[i] = Dot(c, i);

            return result;
        }

        /// <summary>
        /// Intercorrelcation function (ICF)
        /// maxShift - max deviation (left+right)
        /// </summary>
        public Contour InterCorrelation(Contour c, int maxShift)
        {
            Contour result = new Contour(maxShift);
            int i = 0;
            int count = Count;
            while (i < maxShift / 2)
            {
                result.array[i] = Dot(c, i);
                result.array[maxShift - i - 1] = Dot(c, c.Count - i - 1);
                i++;
            }
            return result;
        }

        /// <summary>
        /// Autocorrelation function (ACF)
        /// </summary>
        public unsafe Contour AutoCorrelation(bool normalize)
        {
            int count = Count / 2;
            Contour result = new Contour(count);
            fixed (Complex* ptr = &result.array[0])
            {
                Complex* p = ptr;
                double maxNormaSq = 0;
                for (int i = 0; i < count; i++)
                {
                    *p = Dot(this, i);
                    double normaSq = (*p).NormaSquare;
                    if (normaSq > maxNormaSq)
                        maxNormaSq = normaSq;
                    p++;
                }
                if (normalize)
                {
                    maxNormaSq = Math.Sqrt(maxNormaSq);
                    p = ptr;
                    for (int i = 0; i < count; i++)
                    {
                        *p = new Complex((*p).a / maxNormaSq, (*p).b / maxNormaSq);   //什么意思？
                        p++;
                    }
                }
            }

            return result;
        }

        public void Normalize()
        {
            //find max norma
            double max = FindMaxNorma().Norma;
            //normalize
            if (max > double.Epsilon)
                Scale(1 / max);
        }

        /// <summary>
        /// Finds max norma item
        /// </summary>
        public Complex FindMaxNorma()
        {
            double max = 0d;
            Complex res = default(Complex);
            foreach (var c in array)
                if (c.Norma > max)
                {
                    max = c.Norma;
                    res = c;
                }
            return res;
        }

        /// <summary>
        /// Scalar product
        /// </summary>
        public Complex Dot(Contour c)
        {
            return Dot(c, 0);
        }

        public void Scale(double scale)
        {
            for (int i = 0; i < Count; i++)
                this[i] = scale * this[i];
        }

        // The scale and turn is defined by a complex number μ
        public void Mult(Complex c)
        {
            for (int i = 0; i < Count; i++)
                this[i] = c * this[i];
        }

        public void Rotate(double angle)
        {
            var cosA = Math.Cos(angle);
            var sinA = Math.Sin(angle);
            for (int i = 0; i < Count; i++)
                this[i] = this[i].Rotate(cosA, sinA);
        }

        /// <summary>
        /// Normalized Scalar Product
        /// </summary>
        public Complex NormDot(Contour c)
        {
            var count = this.Count;
            double sumA = 0;
            double sumB = 0;
            double norm1 = 0;
            double norm2 = 0;
            for (int i = 0; i < count; i++)
            {
                var x1 = this[i];
                var x2 = c[i];
                sumA += x1.a * x2.a + x1.b * x2.b;
                sumB += x1.b * x2.a - x1.a * x2.b;
                norm1 += x1.NormaSquare;
                norm2 += x2.NormaSquare;
            }

            double k = 1d / Math.Sqrt(norm1 * norm2);
            return new Complex(sumA * k, sumB * k);
        }

        /// <summary>
        /// Discrete Fourier Transform
        /// </summary>
        public Contour Fourier()
        {
            int count = Count;
            Contour result = new Contour(count);
            for (int m = 0; m < count; m++)
            {
                Complex sum = new Complex(0, 0);
                double k = -2d * Math.PI * m / count;
                for (int n = 0; n < count; n++)
                    sum += this[n].Rotate(k * n);

                result.array[m] = sum;
            }

            return result;
        }
        /// <summary>
        /// ？？？？
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public double Distance(Contour c)
        {
            double n1 = this.Norma;
            double n2 = c.Norma;
            return n1 * n1 + n2 * n2 - 2 * (this.Dot(c).a);
        }

        /// <summary>
        /// Changes length of contour (equalization)
        /// </summary>
        public void Equalization(int newCount)
        {
            if (newCount > Count)
                EqualizationUp(newCount);
            else
                EqualizationDown(newCount);
        }

        private void EqualizationUp(int newCount)
        {
            Complex currPoint = this[0];
            Complex[] newPoint = new Complex[newCount];

            for (int i = 0; i < newCount; i++)
            {
                double index = 1d * i * Count / newCount;
                int j = (int)index;
                double k = index - j;
                if (j == Count - 1)
                    newPoint[i] = this[j];
                else
                    newPoint[i] = this[j] * (1 - k) + this[j + 1] * k;
            }

            array = newPoint;
        }

        private void EqualizationDown(int newCount)
        {
            Complex currPoint = this[0];
            Complex[] newPoint = new Complex[newCount];

            for (int i = 0; i < Count; i++)
                newPoint[i * newCount / Count] += this[i];

            array = newPoint;
        }

        public Point[] GetPoints(Point startPoint)
        {
            Point[] result = new Point[Count + 1];
            PointF sum = startPoint;
            result[0] = Point.Round(sum);
            for (int i = 0; i < Count; i++)
            {
                sum = sum.Offset((float)array[i].a, -(float)array[i].b);
                result[i + 1] = Point.Round(sum);
            }

            return result;
        }

        public RectangleF GetBoundsRect()
        {
            double minX = 0, maxX = 0, minY = 0, maxY = 0;
            double sumX = 0, sumY = 0;
            for (int i = 0; i < Count; i++)
            {
                var v = array[i];
                sumX += v.a;
                sumY += v.b;
                if (sumX > maxX) maxX = sumX;
                if (sumX < minX) minX = sumX;
                if (sumY > maxY) maxY = sumY;
                if (sumY < minY) minY = sumY;
            }

            return new RectangleF((float)minX, (float)minY, (float)(maxX - minX), (float)(maxY - minY));
        }
    }

    [Serializable]
    public struct Complex
    {
        public double a;
        public double b;

        public Complex(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public static Complex FromExp(double r, double angle)
        {
            return new Complex(r * Math.Cos(angle), r * Math.Sin(angle));
        }

        public double Angle
        {
            get
            {
                return Math.Atan2(b, a);
            }
        }

        /// <summary>
        /// 重写ToString(),使按复数的方式输出
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return a + "+i" + b;
        }

        //长度
        public double Norma
        {
            get { return Math.Sqrt(a * a + b * b); }
        }

        public double NormaSquare
        {
            get { return a * a + b * b; }
        }

        public static Complex operator +(Complex x1, Complex x2)
        {
            return new Complex(x1.a + x2.a, x1.b + x2.b);
        }

        public static Complex operator *(double k, Complex x)
        {
            return new Complex(k * x.a, k * x.b);
        }

        public static Complex operator *(Complex x, double k)
        {
            return new Complex(k * x.a, k * x.b);
        }

        public static Complex operator *(Complex x1, Complex x2)
        {
            return new Complex(x1.a * x2.a - x1.b * x2.b, x1.b * x2.a + x1.a * x2.b);
        }

        public double CosAngle()
        {
            return a / Math.Sqrt(a * a + b * b);
        }
        // 两个复数相乘 r1(cosAngle1 + i * sinAngle1)与 r2(cosAngle2 + i * sinAngle2)   
        // 令r2 = 1 ，那么给出Angle2则是将复数旋转了Angle2
        public Complex Rotate(double CosAngle, double SinAngle)
        {
            return new Complex(CosAngle * a - SinAngle * b, SinAngle * a + CosAngle * b);
        }

        public Complex Rotate(double Angle)
        {
            var CosAngle = Math.Cos(Angle);
            var SinAngle = Math.Sin(Angle);
            return new Complex(CosAngle * a - SinAngle * b, SinAngle * a + CosAngle * b);
        }
    }

    public static class PointHelper
    {

        /*
         * 拓展方法:
         * 扩展方法使你能够向现有类型“添加”方法，而无需创建新的派生类型
         * 扩展方法是一种特殊的静态方法，但可以像扩展类型上的实例方法一样进行调用。
         * 扩展方法被定义为静态方法，但它们是通过实例方法语法进行调用的。
         * 它们的第一个参数指定该方法作用于哪个类型，并且该参数以 this 修饰符为前缀。 
         * 仅当你使用 using 指令将命名空间显式导入到源代码中之后，扩展方法才位于范围中。
         */


        //这里相当于为Rectangle类添加了一个新的方法Center(),而不用直接在rectangle类中修改
        public static Point Center(this Rectangle rect) 
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public static int Area(this Rectangle rect)
        {
            return rect.Width * rect.Height;
        }
        public static int Distance(this Point point, Point p)
        {
            return Math.Abs(point.X - p.X) + Math.Abs(point.Y - p.Y);
        }

        //有什么用？？
        public static void NormalizePoints(Point[] points, Rectangle rectangle)
        {
            if (rectangle.Height == 0 || rectangle.Width == 0)
                return;

            Matrix m = new Matrix();
            m.Translate(rectangle.Center().X, rectangle.Center().Y);  //方法Translate：使图形在X轴或Y轴方向移动

            if (rectangle.Width > rectangle.Height)
                m.Scale(1, 1f * rectangle.Width / rectangle.Height);  //方法Scale：在X轴(参数1)或Y轴(参数2)方向对图形放大或缩小。
            else
                m.Scale(1f * rectangle.Height / rectangle.Width, 1);

            m.Translate(-rectangle.Center().X, -rectangle.Center().Y);
            m.TransformPoints(points);
        }

        public static void NormalizePoints2(Point[] points, Rectangle rectangle, Rectangle needRectangle)
        {
            if (rectangle.Height == 0 || rectangle.Width == 0)
                return;

            float k1 = 1f * needRectangle.Width / rectangle.Width;
            float k2 = 1f * needRectangle.Height / rectangle.Height;
            float k = Math.Min(k1, k2);

            Matrix m = new Matrix();
            m.Scale(k, k);
            m.Translate(needRectangle.X / k - rectangle.X, needRectangle.Y / k - rectangle.Y);
            m.TransformPoints(points);
        }

        public static PointF Offset(this PointF p, float dx, float dy)
        {
            return new PointF(p.X + dx, p.Y + dy);
        }

    }

}
