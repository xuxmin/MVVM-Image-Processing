using System;

namespace ContourAnalysisNS
{
    /*
     * Class TemplateFinder implements fast searching of a template for the given contour.
     * Outcome of operation of this class is FoundTemplateDesc which contains an initial contour, 
     * and the template discovered for the given contour. Besides, 
     * FoundTemplateDesc contains similarity rate, angle of rotation and a scale of a contour, relative to a template.
     */
    public class TemplateFinder
    {
        public double minACF = 0.96d;   //等于1就是完全相同了，而这里设定了一个误差值，所以minACF = 0.96d; 
        public double minICF = 0.85d;
        public bool checkICF = true;
        public bool checkACF = true;
        public double maxRotateAngle = Math.PI;
        public int maxACFDescriptorDeviation = 4;  //用数字核对的最大偏差
        public string antiPatternName = "antipattern";

        //通过将sample与模板templates比对，寻找相应的contour，寻找到以后存放在FoundTemplateDesc类中
        public FoundTemplateDesc FindTemplate(Templates templates, Template sample)
        {
            //int maxInterCorrelationShift = (int)(templateSize * maxRotateAngle / Math.PI);
            //maxInterCorrelationShift = Math.Min(templateSize, maxInterCorrelationShift+13);
            double rate = 0;
            double angle = 0;
            Complex interCorr = default(Complex);
            Template foundTemplate = null;
            foreach (var template in templates)
            {
                //
                if (Math.Abs(sample.autoCorrDescriptor1 - template.autoCorrDescriptor1) > maxACFDescriptorDeviation) continue;
                if (Math.Abs(sample.autoCorrDescriptor2 - template.autoCorrDescriptor2) > maxACFDescriptorDeviation) continue;
                if (Math.Abs(sample.autoCorrDescriptor3 - template.autoCorrDescriptor3) > maxACFDescriptorDeviation) continue;
                if (Math.Abs(sample.autoCorrDescriptor4 - template.autoCorrDescriptor4) > maxACFDescriptorDeviation) continue;
                //
                double r = 0;          //可以看作相似度
                if (checkACF)
                {
                    r = template.autoCorr.NormDot(sample.autoCorr).Norma;      //ACF的话不需要FindMaxNorma()
                    if (r < minACF)
                        continue;
                }
                if (checkICF)
                {
                    interCorr = template.contour.InterCorrelation(sample.contour).FindMaxNorma();
                    r = interCorr.Norma / (template.contourNorma * sample.contourNorma);
                    if (r < minICF)
                        continue;
                    if (Math.Abs(interCorr.Angle) > maxRotateAngle)
                        continue;
                }
                if (template.preferredAngleNoMore90 && Math.Abs(interCorr.Angle) >= Math.PI / 2)
                    continue;//unsuitable angle
                //find max rate
                if (r >= rate)
                {
                    rate = r;
                    foundTemplate = template;
                    angle = interCorr.Angle;
                }
            }
            //ignore antipatterns
            if (foundTemplate != null && foundTemplate.name == antiPatternName)
                foundTemplate = null;
            //
            if (foundTemplate != null)
                return new FoundTemplateDesc() { template = foundTemplate, rate = rate, sample = sample, angle = angle };
            else
                return null;
        }
    }

    public class FoundTemplateDesc
    {
        public double rate;    //与模板的相似度
        public Template template;   
        public Template sample;      
        public double angle;  //与模板相比旋转了的角度

        public double scale
        {
            get
            {
                return Math.Sqrt(sample.sourceArea / template.sourceArea);
            }
        }
    }
}
