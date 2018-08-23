using Image_Processing.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Image_Processing
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        
        public MainWindow()
        {
            InitializeComponent();
        }

        public class SourceImage
        {
            public BitmapImage sourceImage { get; set; }
        }

        List<SourceImage> imageList;

        private void btOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif|所有文件|*.*"
            };
            var result = openFileDialog.ShowDialog();
            BitmapImage image;
            if (result == true)
            {
                string path = openFileDialog.FileName;
                image= new BitmapImage(new Uri(path, UriKind.Absolute));
                //Image MainImage = new Image();
                //MainImage.Source = image;
            }
       
           // canvas.Children.Add(ImageControl);
        }

        private void Canny_Selected(object sender, RoutedEventArgs e)
        {
            DataContext = new CannyViewModel();
        }

        private void Basic_Selected(object sender, RoutedEventArgs e)
        {
            DataContext = new BasicViewModel();
        }


    }
}
