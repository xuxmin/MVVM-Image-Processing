using MVVM_Image_Processing.ViewModels;
using MVVM_Image_Processing.Views;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;


namespace MVVM_Image_Processing
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            contentControl.Content = null;

        }
        private BitmapImage _selectedImage;
        private void Canny_Selected(object sender, RoutedEventArgs e)
        {
            if(ImageList.SelectedIndex >= 0)
            {
                _selectedImage = (BitmapImage)ImageList.SelectedItem;

                try
                {
                    FileStream myStream = new FileStream("SelectedImage.txt", FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(myStream, Encoding.GetEncoding("gb2312"));
                    sw.Write(_selectedImage.ToString());
                    sw.Close();
                    myStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            tbHeader.Text = "Canny Edge Detection";
            contentControl.Content = new CannyView();
        }
        private void ContourAnalysis_Selected(object sender, RoutedEventArgs e)
        {
            if(ImageList.SelectedIndex>=0)
            {
                _selectedImage = (BitmapImage)ImageList.SelectedItem;

                try
                {
                    FileStream myStream = new FileStream("SelectedImage.txt", FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(myStream, Encoding.GetEncoding("gb2312"));
                    sw.Write(_selectedImage.ToString());
                    sw.Close();
                    myStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            tbHeader.Text = "Contour Analysis";
            contentControl.Content = new ContourAnalysisView();
        }

        private void ImageList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(ImageList.SelectedItem != null)
            {
                _selectedImage = (BitmapImage)ImageList.SelectedItem;

                try
                {
                    FileStream myStream = new FileStream("SelectedImage.txt", FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(myStream, Encoding.GetEncoding("gb2312"));
                    sw.Write(_selectedImage.ToString());
                    sw.Close();
                    myStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }



            if (lviCanny.IsSelected)
            {
                tbHeader.Text = "Canny Edge Detection";
                contentControl.Content = new CannyView();
            }
            else if(lviContourAnalysis.IsSelected)
            {
                tbHeader.Text = "Contour Analysis";
                contentControl.Content = new ContourAnalysisView();
            }
            else if(lviImageView.IsSelected)
            {
                tbHeader.Text = "Image View";
                contentControl.Content = new ImageView();
            }


        }

        private void lviImageView_Selected(object sender, RoutedEventArgs e)
        {
            if (ImageList.SelectedIndex >= 0)
            {
                _selectedImage = (BitmapImage)ImageList.SelectedItem;

                try
                {
                    FileStream myStream = new FileStream("SelectedImage.txt", FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(myStream, Encoding.GetEncoding("gb2312"));
                    sw.Write(_selectedImage.ToString());
                    sw.Close();
                    myStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            tbHeader.Text = "Image View";
            contentControl.Content = new ImageView();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
            
        }

        private void btMinimize_Click(object sender, RoutedEventArgs e)
        {
            window.WindowState = WindowState.Minimized;

        }

        private void ColorZone_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
