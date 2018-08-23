using MVVM_Image_Processing.ViewModels;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MVVM_Image_Processing.Views
{
    /// <summary>
    /// CannyView.xaml 的交互逻辑
    /// </summary>
    public partial class CannyView : UserControl
    {
        public CannyView()
        {
            InitializeComponent();
        }
        public CannyView(BitmapImage image)
        {
            InitializeComponent();
            DataContext = new CannyViewModel(image);
        }
    }
}
