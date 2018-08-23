using System;
using System.Windows.Media.Imaging;

namespace Image_Processing.Models
{
    public class ImageProcess
    {
        BitmapImage _originalImage;

        #region Creation

        public ImageProcess(BitmapImage iamge)
        {
            _originalImage = iamge;
        }

        protected ImageProcess()
        {
        }

        #endregion // Creation


        #region State Properties

        /// <summary>
        /// 存储需要处理的图片
        /// </summary>
        public BitmapImage OriginalImage{ get; set; }



        #endregion // State Properties
    }
}
