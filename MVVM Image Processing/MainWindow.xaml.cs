using MVVM_Image_Processing.ViewModels;
using MVVM_Image_Processing.Views;
using System;
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
            DataContext =  new MainWindowViewModel();
            
        }
        private BitmapImage _selectedImage;
        private void Canny_Selected(object sender, RoutedEventArgs e)
        {
            contentControl.Content = new CannyView();
        }

        private void ImageList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedImage = (BitmapImage)ImageList.SelectedItem;
            if (Canny.IsSelected)
            {
                contentControl.Content = new CannyView(_selectedImage);
            }
        }
    }
}
