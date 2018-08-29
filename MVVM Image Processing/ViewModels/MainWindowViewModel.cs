using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVVM_Image_Processing.ViewModels
{
    class MainWindowViewModel:ViewModelBase
    {
        #region Fields

        private BitmapImage _selectedImage;
        private BitmapImage _image;
        private ObservableCollection<BitmapImage> _images;
        RelayCommand _openCommand;
        RelayCommand _deleteCommand;

        string directoryPath;
        private FileInfo[] Files;

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
        public BitmapImage SelectedImage
        {
            get
            {
                return _selectedImage;
            }
            set
            {
                _selectedImage = value;
                base.OnPropertyChanged("SelectedImage");
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
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => this.DeleteFile());
                }
                return _deleteCommand;
            }
        }

        public ICommand DirectoryBrowse
        {
            get
            {
                return new RelayCommand(param => DirectoryBrowseExecute());
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

        private void DirectoryBrowseExecute()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog myFolders = new System.Windows.Forms.FolderBrowserDialog();
                myFolders.ShowNewFolderButton = false;

                if (myFolders.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                     directoryPath = myFolders.SelectedPath;
                    _images.Clear();
                    AddItemsToListBox();
                    if (_images.Count == 0)
                    {
                        MessageBox.Show("Selected directory doesn't contains images");
                    }
                    base.OnPropertyChanged("Images");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void AddItemsToListBox()
        {
            string[] extensions = new[] { ".jpg", ".jpeg", ".bmp", ".tiff", ".png" };
            DirectoryInfo dinfo = new DirectoryInfo(directoryPath);
            Files = dinfo.EnumerateFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();

            foreach (FileInfo file in Files)
            {
                _image = new BitmapImage(new Uri(directoryPath + "\\"+ file.Name, UriKind.Absolute));
                _images.Add(_image);
            }
        }

        private void DeleteFile()
        {
            int index = _images.IndexOf(_selectedImage);
            _images.Remove(_selectedImage);

            if(Images.Count == 0)
            {
                _selectedImage = null;
            }
            else if(_images.Count == 1)
            {
                _selectedImage = _images[0];
            }
            else
            {
                if(index == _images.Count)
                {
                    _selectedImage = _images[index - 1];
                }
                else
                {
                    _selectedImage = _images[index];
                }
            }
            base.OnPropertyChanged("Images");
            base.OnPropertyChanged("SelectedImage");
        }
        #endregion

    }
}
