using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using ContourAnalysisNS;
using System.IO;
using System.Drawing;

namespace MVVM_Image_Processing
{
    /// <summary>
    /// ShowContoursForm.xaml 的交互逻辑
    /// </summary>
    public partial class ShowContoursForm : Window
    {
        public ShowContoursForm()
        {
            InitializeComponent();
        }

        Templates templates;
        Templates samples;
        public Template selectedTemplate;
        Bitmap bmp;
        int RowCount;

        public ShowContoursForm(Templates templates, Templates samples, IImage image)
        {
            if (image == null)
                return;
            InitializeComponent();
            this.templates = templates;
            this.samples = samples;

            this.samples = new Templates();
            foreach (var sample in samples)
                this.samples.Add(sample);

            //dgvContours.= samples.Count;
            RowCount = samples.Count;
            //SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);


            //some magic
            string fileName = @"D:\Project\ContourAnalysis\Temp\" + "temp.bmp";
            image.Save(fileName);
            bmp = (Bitmap)System.Drawing.Image.FromFile(fileName);


            ShowGrid();
        }
        public class MyDataObject
        {
            public int Number { get; set; }
            public BitmapImage image { get; set; }
        }

        public BitmapImage ConvertBitmap(System.Drawing.Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        private void ShowGrid()
        {
            List<MyDataObject> list = new List<MyDataObject>();

            MyDataObject myDataObject = new MyDataObject();

            Template template;
            int bmpWidth = 600;
            int bmpHeight = 200;


            for (int i = 0; i < RowCount; i++)
            {
                Bitmap temp = new Bitmap(bmpWidth, bmpHeight);

                Graphics gp = Graphics.FromImage(temp);

                template = samples[i];

                var rect = new System.Drawing.Rectangle(0, 0, (bmpWidth - 24) / 2, bmpHeight);
                rect.Inflate(-20, -20);
                System.Drawing.Rectangle boundRect = template.contour.SourceBoundingRect;
                float k1 = 1f * rect.Width / boundRect.Width;
                float k2 = 1f * rect.Height / boundRect.Height;
                float k = Math.Min(k1, k2);
                //绘制bmp的指定部分，就是将那一块轮廓分割出来
                gp.DrawImage(bmp,
                    new System.Drawing.Rectangle(rect.X, rect.Y, (int)(boundRect.Width * k), (int)(boundRect.Height * k)),
                    boundRect, GraphicsUnit.Pixel);

                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(0, 0, bmpWidth, bmpHeight);
                template.Draw(gp, rectangle);

                list.Add(new MyDataObject() { Number = i, image = ConvertBitmap(temp) });

            }
            this.dgvContours.ItemsSource = list;
            //dgvContours.ItemsSource = dt.DefaultView;

            dgvContours.GridLinesVisibility = DataGridGridLinesVisibility.All;
        }

        private void dgvContours_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tbTemplateName.Text == "<template name>")
                MessageBox.Show("Enter template name");
            else
                try
                {
                    int i = dgvContours.SelectedIndex;
                    samples[i].name = tbTemplateName.Text;
                    templates.Add(samples[i]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }


        private void tbTemplateName_MouseEnter(object sender, MouseEventArgs e)
        {
            tbTemplateName.Foreground = System.Windows.Media.Brushes.Black;
            if (tbTemplateName.Text == "<template name>")
                tbTemplateName.Text = "";
        }
    }
}
