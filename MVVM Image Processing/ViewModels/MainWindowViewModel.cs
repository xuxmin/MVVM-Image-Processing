using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVVM_Image_Processing.ViewModels
{
    class MainWindowViewModel:ViewModelBase
    {
        #region Fields

        private BitmapImage _image;
        private ObservableCollection<BitmapImage> _images;
        RelayCommand _openCommand;
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            _images = new ObservableCollection<BitmapImage>();
        }

        #endregion

        #region Property

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
        public ObservableCollection<BitmapImage> Images
        {
            get
            {
                //var result = new ObservableCollection<BitmapImage>();

                return _images;
            }
            set
            {
                _images = value;
                base.OnPropertyChanged("Images");
            }
        }

        #endregion

        #region Command

        public ICommand OpenCommand
        {
            get
            {
                if(_openCommand == null)
                {
                    _openCommand = new RelayCommand(param => this.OpenFile());
                }
                return _openCommand;
            }
        }

        #endregion

        #region  Methods
        private void OpenFile()
        {
            if (_images == null) _images = new ObservableCollection<BitmapImage>();

            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif|所有文件|*.*"
            };
            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string path = openFileDialog.FileName;
                _image = new BitmapImage(new Uri(path, UriKind.Absolute));               
                _images.Add(_image);                
            }            
            // base.OnPropertyChanged("SelectedImage");
        }

        #endregion
    }
}
