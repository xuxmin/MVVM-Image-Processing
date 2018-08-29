using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ContourAnalysisNS;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;

namespace MVVM_Image_Processing.ViewModels
{
    class ContourAnalysisViewModel:ViewModelBase
    {
        #region Fields

        int _contourLength;
        int _contourArea;
        int _maxACFDesc;
        double _minACF;
        double _minICF;
        bool _showContours;
        string templateFile;
        Dictionary<string, System.Drawing.Image> AugmentedRealityImages = new Dictionary<string, System.Drawing.Image>();

        BitmapImage _originFrame; //原图片
        BitmapImage _frame;   //显示的图片

        RelayCommand _recognizeCommand;
        RelayCommand _openTemplateCommand;
        RelayCommand _templateEditorCommand;
        RelayCommand _createTemplateCommand;

        ImageProcessor processor;     //

        #endregion

        #region Property
        public BitmapImage Frame
        {
            set
            {
                _frame = value;
                base.OnPropertyChanged("Frame");
            }
            get
            {
                return _frame;

            }
        }


        public int ContourLength
        {
            get
            {
                return _contourLength;
            }
            set
            {
                if(value > 400)
                {
                    _contourLength = 400;
                    MessageBox.Show("输入的值超出范围");
                }
                else if ( value < 0 )
                {
                    _contourLength = 0;
                    MessageBox.Show("输入的值不符合规定");
                }
                else
                {
                    _contourLength = (int)value;
                }
                
                base.OnPropertyChanged("ContourLength");
            }
        }

        public int ContourArea
        {
            get
            {
                return _contourArea;
            }
            set
            {
                if (value > 400)
                {
                    _contourArea = 400;
                    MessageBox.Show("输入的值超出范围");
                }
                else if (value < 0)
                {
                    _contourArea = 0;
                    MessageBox.Show("输入的值不符合规定");
                }
                else
                {
                    _contourArea = (int)value;

                }
                base.OnPropertyChanged("ContourArea");
            }
        }

        public int MaxACFDesc
        {
            get
            {
                return _maxACFDesc;
            }
            set
            {
                if (value > 50)
                {
                    _maxACFDesc = 400;
                    MessageBox.Show("输入的值超出范围");
                }
                else if (value < 0)
                {
                    _maxACFDesc = 0;
                    MessageBox.Show("输入的值不符合规定");
                }
                else
                {
                    _maxACFDesc = (int)value;
                }
                base.OnPropertyChanged("MaxACFDesc");
            }
        }
        
        public double MinACF
        {
            get
            {
                return _minACF;
            }
            set
            {
                if (value > 1)
                {
                    _minACF = 1;
                    MessageBox.Show("输入的值不符合规定");

                }
                else if (value < 0.2)
                {
                    _minACF = 0.2;
                    MessageBox.Show("输入的值超出范围");
                }
                else
                {
                    _minACF = value;
                }
                base.OnPropertyChanged("MinACF");
            }
        }
        public double MinICF
        {
            get
            {
                return _minICF;
            }
            set
            {
                if (value > 1)
                {
                    _minICF = 1;
                    MessageBox.Show("输入的值不符合规定");
                }
                else if (value < 0.2)
                {
                    _minICF = 0.2;
                    MessageBox.Show("输入的值超出范围");
                }
                else
                {
                    _minICF = value;
                }                
                base.OnPropertyChanged("MinICF");
            }
        }

        public bool ShowContours
        {
            get
            {
                return _showContours;
            }
            set
            {
                _showContours = value;
                base.OnPropertyChanged("ShowContours");
            }
        }



        #endregion

        #region Constructor

        public ContourAnalysisViewModel()
        {
            
            try
            {
                if (File.Exists("SelectedImage.txt"))
                {
                    StreamReader reader = new StreamReader("SelectedImage.txt", Encoding.GetEncoding("gb2312"));
                    string path = reader.ReadToEnd();
                    reader.Close();
                    File.Delete("SelectedImage.txt");
                    path = System.Text.RegularExpressions.Regex.Replace(path, "file:///","");
                    _frame = new BitmapImage(new Uri(path, UriKind.Absolute));
                    _originFrame = new BitmapImage(new Uri(path, UriKind.Absolute));
                }
                //Settings
                _contourLength = 15;
                _contourArea = 10;
                _maxACFDesc = 4;
                _minACF = 0.96;
                _minICF = 0.85;
                //create image preocessor
                processor = new ImageProcessor();
                //load default templates
                templateFile ="Tahoma.bin";  //获取程序集的基目录+文件名
                LoadTemplates(templateFile);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #endregion

        #region Command
        public ICommand RecognizeCommand
        {
            get
            {
                if (_recognizeCommand == null)
                {
                    _recognizeCommand = new RelayCommand(param => this.Recognize());
                }
                return _recognizeCommand;
            }
        }

        public ICommand OpenTemplateCommand
        {
            get
            {
                if (_openTemplateCommand == null)
                {
                    _openTemplateCommand = new RelayCommand(param => this.OpenTemplates());
                }
                return _openTemplateCommand;
            }
        }
        public ICommand CreateTemplateCommand
        {
            get
            {
                if (_createTemplateCommand == null)
                {
                    _createTemplateCommand = new RelayCommand(param => this.CreateTemplates());
                }
                return _createTemplateCommand;
            }
        }

        public ICommand TemplateEditorCommand
        {
            get
            {
                if (_templateEditorCommand == null)
                {
                    _templateEditorCommand = new RelayCommand(param => this.TemplateEditor());
                }
                return _templateEditorCommand;
            }
        }

        #endregion

        #region  Methods

        private void Recognize()
        {
            //settings
            processor.minContourLength = _contourLength;
            processor.minContourArea = _contourArea;
            processor.finder.maxACFDescriptorDeviation = _maxACFDesc;
            processor.finder.minACF = _minACF;
            processor.finder.minICF = _minICF;


            if (_originFrame == null) return;
            Image<Bgr, byte> frame = new Image<Bgr, byte>(BitmapConvert.BitmapImageToBitmap( _originFrame));

            //process image
            processor.ProcessImage(frame);


            Font font = new Font("Times New Roman", 24);//16

            Bitmap bmpFrame = frame.ToBitmap();

            Graphics e = Graphics.FromImage(bmpFrame);

            //e.DrawString(lbFPS.Content.ToString(), new Font("Times New Roman", 16), Brushes.Yellow, new PointF(1, 1));

            Brush bgBrush = new SolidBrush(Color.Blue);
            Brush foreBrush = new SolidBrush(Color.Red);
            Pen borderPen = new Pen(Color.FromArgb(150, 0, 255, 0));
            //
            if (_showContours)
                foreach (var contour in processor.contours)
                    if (contour.Size > 1)
                        e.DrawLines(Pens.Red, contour.ToArray());
            //
            lock (processor.foundTemplates)
                foreach (FoundTemplateDesc found in processor.foundTemplates)
                {
                    //做什么？？
                    if (found.template.name.EndsWith(".png") || found.template.name.EndsWith(".jpg"))
                    {
                        DrawAugmentedReality(found, e);
                        continue;
                    }

                    Rectangle foundRect = found.sample.contour.SourceBoundingRect;
                    System.Drawing.Point p1 = new System.Drawing.Point((foundRect.Left + foundRect.Right) / 2, foundRect.Top);
                    string text = found.template.name;
                
                    e.DrawRectangle(borderPen, foundRect);
                    e.DrawString(text, font, bgBrush, new PointF(p1.X + 1 - font.Height / 3, p1.Y + 1 - font.Height));
                    e.DrawString(text, font, foreBrush, new PointF(p1.X - font.Height / 3, p1.Y - font.Height));
                }

            _frame = BitmapConvert.toBitmapImage(bmpFrame);

            base.OnPropertyChanged("Frame");
        }
        private void DrawAugmentedReality(FoundTemplateDesc found, Graphics gr)
        {
            string fileName = Path.GetDirectoryName(templateFile) + "\\" + found.template.name;
            if (!AugmentedRealityImages.ContainsKey(fileName))
            {
                if (!File.Exists(fileName)) return;
                AugmentedRealityImages[fileName] = System.Drawing.Image.FromFile(fileName);
            }
            System.Drawing.Image img = AugmentedRealityImages[fileName];
            System.Drawing.Point p = found.sample.contour.SourceBoundingRect.Center();
            var state = gr.Save();
            gr.TranslateTransform(p.X, p.Y);
            gr.RotateTransform((float)(180f * found.angle / Math.PI));
            gr.ScaleTransform((float)(found.scale), (float)(found.scale));
            gr.DrawImage(img, new System.Drawing.Point(-img.Width / 2, -img.Height / 2));
            gr.Restore(state);
        }
        private void LoadTemplates(string fileName)
        {
            try
            {
                //将指定的流反序列化为对象图形
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    processor.templates = (Templates)new BinaryFormatter().Deserialize(fs);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void OpenTemplates()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Templates(*.bin)|*.bin";
                if (ofd.ShowDialog() ?? false)
                {
                    templateFile = ofd.FileName;
                    LoadTemplates(templateFile);
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }
        private void CreateTemplates()
        {
            if(_frame != null)
            {
                Image<Bgr, byte> image = new Image<Bgr, byte>(BitmapConvert.BitmapImageToBitmap(_frame));
                new ShowContoursForm(processor.templates, processor.samples, image).ShowDialog();
            }
            else
            {
                MessageBox.Show("要先识别一张图片");
            }

        }
        private void TemplateEditor()
        {
            new TemplateEditor(processor.templates).ShowDialog();
        }

        #endregion

    }
}
