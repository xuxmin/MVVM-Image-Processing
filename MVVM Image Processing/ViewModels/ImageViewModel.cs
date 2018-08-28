using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MVVM_Image_Processing.ViewModels
{
    class ImageViewModel : ViewModelBase
    {
        BitmapImage _image;

        public BitmapImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                base.OnPropertyChanged("Image");
            }
        }

        public ImageViewModel()
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
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
