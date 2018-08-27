
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Controls;


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

        private void tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9-]+");

            e.Handled = re.IsMatch(e.Text);
        }
        private void tb_PreviewTextFloatInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");

            e.Handled = re.IsMatch(e.Text);
        }
    }
}
