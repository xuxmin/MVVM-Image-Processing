using Image_Processing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVVM_Image_Processing.ViewModels
{
    class CannyViewModel:ViewModelBase
    {
        #region Fields

       private BitmapImage _image;

        #endregion


        #region Constructor
        public CannyViewModel(BitmapImage image)
        {
            _image = image;
        }

        #endregion

        #region Properties
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

        #endregion

    }
}
