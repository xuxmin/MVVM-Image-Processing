using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ContourAnalysisNS
{
    public class ImageProcessor
    {
        //settings
        public bool equalizeHist = false;    //图像直方图均衡化，可用来提高对比度，使边界更明显
        public bool noiseFilter = false;     //canny边缘检测
        public int cannyThreshold = 50;      //canny检测的阈值
        public bool blur = true;             //高斯模糊
        public int adaptiveThresholdBlockSize = 4;       //用来计算阈值的象素邻域大小
        public double adaptiveThresholdParameter = 1.2d; //自适应阈值算法最后减去的常数
        //函数cvAdaptiveThreshold的确可以将灰度图像二值化，但它的主要功能应该是边缘提取，并且参数param1主要是用来控制边缘的类型和粗细的，
        public bool addCanny = true;
        public bool filterContoursBySize = true;      //过滤轮廓。。滤去小于minContourLength与minContourArea与minFormFactor的轮廓
        public bool onlyFindContours = false;         //只找轮廓，不与模板匹配，这个开关用于生成模板的时候
        public int minContourLength = 15;
        public int minContourArea = 10;
        public double minFormFactor = 0.5;     //Area/Length
        //
        //public List<Contour<Point>> contours;
        public List<VectorOfPoint> contours;     //从图像中获得的轮廓

        public Templates templates = new Templates();      //用来验证的模板集合
        public Templates samples = new Templates();        //需要被验证的样本
        public List<FoundTemplateDesc> foundTemplates = new List<FoundTemplateDesc>();      //已经寻找到的
        public TemplateFinder finder = new TemplateFinder();
        public Image<Gray, byte> binarizedFrame;         //二值化后的图片

        public void ProcessImage(Image<Bgr, byte> frame)   //先转换成灰度图再处理
        {
            ProcessImage(frame.Convert<Gray, Byte>());
        }
        public void ProcessImage(Image<Gray, byte> grayFrame)
        {            
            if (equalizeHist)
                grayFrame._EqualizeHist();//autocontrast
            
            //高斯平滑
            Image<Gray, byte> smoothedGrayFrame = grayFrame.PyrDown();
            smoothedGrayFrame = smoothedGrayFrame.PyrUp();
            
            //canny
            Image<Gray, byte> cannyFrame = null;
            if (noiseFilter)
                cannyFrame = smoothedGrayFrame.Canny(cannyThreshold, cannyThreshold);
            
            //smoothing
            if (blur)
                grayFrame = smoothedGrayFrame;

            //局部自适应阈值二值化，阈值本身作为了一个变量，检测更有效
            //CvInvoke.cvAdaptiveThreshold(grayFrame, grayFrame, 255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1, adaptiveThresholdParameter);
            CvInvoke.AdaptiveThreshold(grayFrame, grayFrame, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1, adaptiveThresholdParameter);
            
            //
            grayFrame._Not();
            //
            if (addCanny)
                if (cannyFrame != null)
                    grayFrame._Or(cannyFrame);    //试验了一下，这样轮廓会更加明显
            //
            this.binarizedFrame = grayFrame;

            //dilate canny contours for filtering
            //膨胀操作，使白色区域的像素增加一圈，看起来轮廓更细了
            if (cannyFrame != null)
                cannyFrame = cannyFrame.Dilate(3);

            //find contours
            VectorOfVectorOfPoint sourceContours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(grayFrame, sourceContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //var sourceContours = grayFrame.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
            
            //filter contours
            contours = FilterContours(sourceContours, cannyFrame, grayFrame.Width, grayFrame.Height);


            //find templates
            lock (foundTemplates)
                foundTemplates.Clear();
            samples.Clear();

            lock (templates)   //多线程，异步执行
                Parallel.ForEach<VectorOfPoint>(contours, (contour) =>
                {
                    var arr = contour.ToArray();
                    Template sample = new Template(arr, CvInvoke.ContourArea(contour), samples.templateSize);
                    lock (samples)
                        samples.Add(sample);

                    if (!onlyFindContours)
                    {
                        FoundTemplateDesc desc = finder.FindTemplate(templates, sample);          

                        if (desc != null)
                            lock (foundTemplates)
                                foundTemplates.Add(desc);
                    }
                }
                );
            //
            FilterByIntersection(ref foundTemplates);
        }
        /// <summary>
        /// 除去一些含在其他轮廓中的轮廓
        /// </summary>
        /// <param name="templates"></param>
        private static void FilterByIntersection(ref List<FoundTemplateDesc> templates)
        {
            //sort by area  从小到大
            templates.Sort(new Comparison<FoundTemplateDesc>((t1, t2) => -t1.sample.contour.SourceBoundingRect.Area().CompareTo(t2.sample.contour.SourceBoundingRect.Area())));
            //exclude templates inside other templates 有一些轮廓在其他轮廓中，这样的轮廓要除去
            HashSet<int> toDel = new HashSet<int>();
            for (int i = 0; i < templates.Count; i++)
            {
                if (toDel.Contains(i))
                    continue;
                Rectangle bigRect = templates[i].sample.contour.SourceBoundingRect;      
                int bigArea = templates[i].sample.contour.SourceBoundingRect.Area();
                bigRect.Inflate(4, 4);    //将矩形放大
                for (int j = i + 1; j < templates.Count; j++)
                {
                    if (bigRect.Contains(templates[j].sample.contour.SourceBoundingRect))
                    {
                        double a = templates[j].sample.contour.SourceBoundingRect.Area();
                        if (a / bigArea > 0.9d)
                        {
                            //choose template by rate
                            if (templates[i].rate > templates[j].rate)
                                toDel.Add(j);
                            else
                                toDel.Add(i);
                        }
                        else//delete tempate
                            toDel.Add(j);
                    }
                }
            }
            List<FoundTemplateDesc> newTemplates = new List<FoundTemplateDesc>();
            for (int i = 0; i < templates.Count; i++)
                if (!toDel.Contains(i))
                    newTemplates.Add(templates[i]);
            templates = newTemplates;
        }
        /// <summary>
        /// 按照特定的要求滤去一些轮廓
        /// </summary>
        /// <param name="contours"></param>
        /// <param name="cannyFrame"></param>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        /// <returns></returns>
        private List<VectorOfPoint> FilterContours(VectorOfVectorOfPoint contours, Image<Gray, byte> cannyFrame, int frameWidth, int frameHeight)
        {
            int maxArea = frameWidth * frameHeight / 5;
            
            List<VectorOfPoint> result = new List<VectorOfPoint>();
          
            int count = contours.Size; 
            for (int i = 0; i < count; i++)
            {
                using (VectorOfPoint currContour = contours[i])
                {
                    if (filterContoursBySize)
                        if (currContour.Size < minContourLength
                            || CvInvoke.ContourArea(currContour) < minContourArea
                            || CvInvoke.ContourArea(currContour) > maxArea
                            || CvInvoke.ContourArea(currContour) / currContour.Size <= minFormFactor)
                            continue;

                    if (noiseFilter)    //有什么用？？
                    {
                        Point p1 = currContour[0];
                        Point p2 = currContour[(currContour.Size / 2) % currContour.Size];
                        if (cannyFrame[p1].Intensity <= double.Epsilon && cannyFrame[p2].Intensity <= double.Epsilon)
                            continue;
                    }

                    result.Add(currContour);
                }
            }
            return result;
        }
    }
}
