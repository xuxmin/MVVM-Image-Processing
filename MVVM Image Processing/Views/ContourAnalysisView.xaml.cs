using MVVM_Image_Processing.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MVVM_Image_Processing.Views
{
    /// <summary>
    /// ContourAnalysisView.xaml 的交互逻辑
    /// </summary>
    public partial class ContourAnalysisView : UserControl
    {
        public ContourAnalysisView()
        {
            InitializeComponent();
        }

        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            Regex re = new Regex("[^0-9.-]+");

            e.Handled = re.IsMatch(e.Text);

        }



    }
}
