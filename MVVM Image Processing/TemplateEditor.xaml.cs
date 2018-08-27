using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ContourAnalysisNS;
using System.Drawing;
using Emgu.CV;
using System.IO;

namespace MVVM_Image_Processing
{
    /// <summary>
    /// TemplateEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateEditor : Window
    {

        Templates templates;
        //int RowCount;
        List<Temp> temps = new List<Temp>();
        public class Temp
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }

        public TemplateEditor()
        {
            InitializeComponent();
        }
        public TemplateEditor(Templates templates)
        {
            InitializeComponent();

            this.templates = templates;
            templates.Sort((t1, t2) => t1.name.CompareTo(t2.name));

            for (int i = 0; i < templates.Count; i++)
            {
                temps.Add(new Temp() { ID = i.ToString(), Name = templates[i].name });
            }

            dgvTemplates.ItemsSource = temps;
        }


        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgvTemplates.SelectedCells.Count > 0)
            {
                int iRow = dgvTemplates.SelectedIndex;
                if (iRow >= 0 && iRow < templates.Count)
                {
                    templates.RemoveAt(iRow);

                    temps.RemoveAt(iRow);

                    dgvTemplates.Items.Refresh();   //重要哦
                }
            }
        }

        private void dgvTemplates_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;    //设置行表头的内容值  
        }

        //这个要改一下
        public static class BitmapConvert
        {
            [System.Runtime.InteropServices.DllImport("gdi32")]
            private static extern int DeleteObject(IntPtr o);

            public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
            {
                if (bitmap == null)
                    throw new ArgumentNullException("bitmap");

                lock (bitmap)
                {
                    IntPtr hBitmap = bitmap.GetHbitmap();

                    try
                    {
                        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                    finally
                    {
                        DeleteObject(hBitmap);
                    }
                }
            }

            public static BitmapSource ToBitmapSource(IImage image)
            {
                using (System.Drawing.Bitmap source = image.Bitmap)
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(ptr);
                    return bs;
                }
            }
            public static BitmapImage toBitmapImage(System.Drawing.Bitmap bitmap)
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
        }

        private void dgvTemplates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvTemplates.SelectedCells.Count > 0)
            {
                int iRow = dgvTemplates.SelectedIndex;
                if (iRow >= 0 && iRow < templates.Count)
                {
                    //Refresh();

                    Bitmap bmp = new Bitmap(277, 226);
                    Graphics gp = Graphics.FromImage(bmp);
                    templates[iRow].Draw(gp, new System.Drawing.Rectangle(0, 0, 277, 226));
                    image1.Source = BitmapConvert.CreateBitmapSourceFromBitmap(bmp);
                    cbPreferredAngle.IsChecked = templates[iRow].preferredAngleNoMore90;
                }
            }
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (dgvTemplates.SelectedCells.Count > 0)
            {
                int iRow = dgvTemplates.SelectedIndex;
                if (iRow >= 0 && iRow < templates.Count)
                {
                    templates[iRow].preferredAngleNoMore90 = cbPreferredAngle.IsChecked ?? false;
                }
            }
        }
    }
}
