using System.Windows.Media.Imaging;
using CannyEdgeDetection;
using System.Windows.Input;
using System.Drawing;
using System.Windows;
using System.IO;
using System;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MVVM_Image_Processing.ViewModels
{
    class CannyViewModel : ViewModelBase
    {
        #region Fields

        private BitmapImage _image;
        private BitmapImage _cannyImage;
        private BitmapImage _GFImage;
        private BitmapImage _NMSImage;
        private BitmapImage _WEImage;
        private BitmapImage _SEImage;
        private int _thrHigh;
        private int _thrLow;
        private int _kernelSize;
        private float _sigma;
        private int _thresh;
        private int _threshLinking;
        private int _blockSize;
        private int _parameter;
        private bool _smooth;
        private bool _equalizeHist;
        private bool _isChecked;
        private int _progressValue;
        RelayCommand _cannyCommand;

        #endregion

        #region Constructor
        public CannyViewModel()
        {
            try
            {
                if (File.Exists("SelectedImage.txt"))
                {
                    StreamReader reader = new StreamReader("SelectedImage.txt", Encoding.GetEncoding("gb2312"));
                    string path = reader.ReadToEnd();
                    reader.Close();
                    File.Delete("SelectedImage.txt");
                    _image = new BitmapImage(new Uri(path, UriKind.Absolute));
                }

            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            _sigma = 1;
            _thrHigh = 52;
            _thrLow = 25;
            _kernelSize = 5;
            _thresh = 50;
            _threshLinking = 50;
            _blockSize = 4;
            _parameter = 8;
            _smooth = false;
            _equalizeHist = false;
            _progressValue = 0;
        }

        #endregion

        #region Properties
        public BitmapImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public BitmapImage CannyImage
        {
            get
            {
                return _cannyImage;
            }
            set
            {
                _cannyImage = value;
                OnPropertyChanged("CannyImage");
            }
        }
        public BitmapImage GFImage
        {
            get
            {
                return _GFImage;
            }
            set
            {
                _GFImage = value;
                base.OnPropertyChanged("GFImage");
            }
        }
        public BitmapImage NMSImage
        {
            get
            {
                return _NMSImage;
            }
            set
            {
                _NMSImage = value;
                base.OnPropertyChanged("NMSImage");
            }
        }
        public BitmapImage SEImage
        {
            get
            {
                return _SEImage;
            }
            set
            {
                _SEImage = value;
                base.OnPropertyChanged("SEImage");
            }
        }
        public BitmapImage WEImage
        {
            get
            {
                return _WEImage;
            }
            set
            {
                _WEImage = value;
                base.OnPropertyChanged("WEImage");
            }
        }
        public int ThrHigh
        {
            get
            {
                return _thrHigh;
            }
            set
            {
                _thrHigh = value;
                base.OnPropertyChanged("ThrHigh");
            }
        }
        public int ThrLow
        {
            get
            {
                return _thrLow;
            }
            set
            {
                _thrLow = value;
                base.OnPropertyChanged("ThrLow");
            }
        }
        public int KernelSize
        {
            get
            {
                return _kernelSize;
            }
            set
            {
                _kernelSize = value;
                base.OnPropertyChanged("KernelSize");
            }
        }
        public float Sigma
        {
            get
            {
                return _sigma;
            }
            set
            {
                _sigma = value;
                base.OnPropertyChanged("Sigma");
            }
        }
        public int Thresh
        {
            get
            {
                return _thresh;
            }
            set
            {
                _thresh = value;
                base.OnPropertyChanged("Thresh");
            }
        }
        public int ThreshLinking
        {
            get
            {
                return _threshLinking;
            }
            set
            {
                _threshLinking = value;
                base.OnPropertyChanged("ThreshLinking");
            }
        }
        public int BlockSize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                _blockSize = value;
                base.OnPropertyChanged("BlockSize");
            }
        }
        public int Parameter
        {
            get
            {
                return _parameter;
            }
            set
            {
                _parameter = value;
                base.OnPropertyChanged("Parameter");
            }
        }
        public bool Smooth
        {
            get
            {
                return _smooth;
            }
            set
            {
                _smooth = value;
                base.OnPropertyChanged("Smooth");
            }
        }
        public bool EqualizeHist
        {
            get
            {
                return _equalizeHist;
            }
            set
            {
                _equalizeHist = value;
                base.OnPropertyChanged("EqualizeHist");
            }
        }
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                base.OnPropertyChanged("IsChecked");
            }
        }
        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                base.OnPropertyChanged("ProgressValue");
            }
        }

        #endregion

        #region Command
        public ICommand CannyCommand
        {
            get
            {
                if (_cannyCommand == null)
                {
                    _cannyCommand = new RelayCommand(param => this.EdgeDetect());
                }
                return _cannyCommand;
            }
        }

        #endregion

        #region  Methods
        private void EdgeDetect()
        {
            if (_image != null)
            {
                try
                {
                    if (_isChecked)
                    {
                        _progressValue = 0;
                        base.OnPropertyChanged("ProgressValue");

                        Bitmap _bmpImage = BitmapConvert.BitmapImageToBitmap(_image);

                        Bitmap edge;
                        Canny.DetectCannyEdges(_bmpImage, out edge, _kernelSize, _sigma, _thrHigh, _thrLow);

                        _cannyImage = BitmapConvert.toBitmapImage(edge);
                        base.OnPropertyChanged("CannyImage");

                        Bitmap gf, np, se, we;
                        Canny.GaussianFilter(_bmpImage, out gf, _kernelSize, _sigma);
                        _GFImage = BitmapConvert.toBitmapImage(gf);
                        base.OnPropertyChanged("GFImage");

                        Canny.Suppression(_bmpImage, out np, _kernelSize, _sigma);
                        _NMSImage = BitmapConvert.toBitmapImage(np);
                        base.OnPropertyChanged("NMSImage");

                        Canny.Threshold(_bmpImage, out se, out we, _kernelSize, _sigma, _thrHigh, _thrLow);
                        _WEImage = BitmapConvert.toBitmapImage(we);
                        base.OnPropertyChanged("WEImage");
                        _SEImage = BitmapConvert.toBitmapImage(se);
                        base.OnPropertyChanged("SEImage");


                    }
                    else
                    {
                        Bitmap _bmpImage = BitmapConvert.BitmapImageToBitmap(_image);
                        //获得灰度图
                        Image<Gray, byte> grayFrame = new Image<Gray, byte>(_bmpImage);


                        if (_equalizeHist)
                            grayFrame._EqualizeHist();//autocontrast

                        //高斯平滑
                        Image<Gray, byte> smoothedGrayFrame = grayFrame.PyrDown();
                        smoothedGrayFrame = smoothedGrayFrame.PyrUp();

                        //canny
                        Image<Gray, byte> cannyFrame = null;
                        if (_smooth)
                        {
                            cannyFrame = smoothedGrayFrame.Canny(_thresh, _threshLinking);
                            grayFrame = smoothedGrayFrame;
                        }
                        else
                        {
                            grayFrame = grayFrame.Canny(_thresh, _threshLinking);
                        }

                        //局部自适应阈值二值化，阈值本身作为了一个变量，检测更有效
                        //CvInvoke.cvAdaptiveThreshold(grayFrame, grayFrame, 255, Emgu.CV.CvEnum.ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY, adaptiveThresholdBlockSize + adaptiveThresholdBlockSize % 2 + 1, adaptiveThresholdParameter);
                        CvInvoke.AdaptiveThreshold(grayFrame, grayFrame, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, _blockSize + _blockSize% 2 + 1, _parameter);

                        //
                        grayFrame._Not();
                        //
                        if (cannyFrame != null)
                            grayFrame._Or(cannyFrame);    //试验了一下，这样轮廓会更加明显


                        _cannyImage = BitmapConvert.toBitmapImage(grayFrame.ToBitmap());
                        base.OnPropertyChanged("CannyImage");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }

        #endregion
    }
}
