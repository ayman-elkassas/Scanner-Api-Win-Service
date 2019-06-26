using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ClientScanner.TiffImage
{

    static class TifImageErrorMessage
    {
        public const string ERROR_SAVE_IMG = "خطأ اثناء حفظ الصورة";
        public const string ERROR_APPEND_IMG = "خطأ اثناء اضافة الصورة";
    }

    /// <summary>
    /// Abstracts TIFF image
    /// </summary>
    public class TiffImage
    {

        List<Bitmap> _imgs;
        

        /// <summary>
        /// Creates a tiff image from a memory stream
        /// </summary>
        /// <param name="tifstream"></param>
        public TiffImage(Stream tifstream)
        {
            _imgs = new List<Bitmap>();
            SplitBitMaps(tifstream);
        }
        /// <summary>
        /// Saves TIFF image at the given directory
        /// </summary>
        /// <param name="dir">save directory</param>
        public void Save(string dir)
        {
            try
            {
                 
                //Encoder encoder = Encoder.SaveFlag;
                ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
                var encoderParameters = GetEncoderParams();

                Bitmap firstImg = _imgs[0];

                firstImg.Save(dir, encoderInfo, encoderParameters);

                encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

                for (int i = 1; i < _imgs.Count; i++)
                {
                    firstImg.SaveAdd(_imgs[i], encoderParameters);
                }
                encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
                firstImg.SaveAdd(encoderParameters);
            }
            catch (System.Exception)
            {

                throw new System.Exception(TifImageErrorMessage.ERROR_SAVE_IMG);
            }
        }

        /// <summary>
        /// Appends the tiff image to another tiff image at the given directory.
        /// In case no image exist at the given directory, a copy of this tiff image will\
        /// be created at the given dir.
        /// </summary>
        /// <param name="dir"></param>
        public void Append(string dir,int index=-1)
        {
            try
            {
                if (!File.Exists(dir))
                {
                    Save(dir);
                    return;
                }

                var ms00 = new MemoryStream();
                using (var fs0 = new FileStream(dir, FileMode.Open))
                {
                    fs0.CopyTo(ms00);
                    AppendBack(new TiffImage(ms00),index);
                }
                    //AppendBack(new TiffImage(fs));
                    //_imgs.Reverse();

                ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
                var encoderParameters = GetEncoderParams();

                Bitmap firstImg = _imgs[0];
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    firstImg.Save(ms, encoderInfo, encoderParameters);

                    encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

                    for (int i = 1; i < _imgs.Count; i++)
                    {
                        firstImg.SaveAdd(_imgs[i], encoderParameters);
                    }
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
                    firstImg.SaveAdd(encoderParameters);

                    firstImg.Dispose();
                    foreach (var item in _imgs)
                    {
                        item.Dispose();
                    }
                    bytes = ms.ToArray();
                }
                using (var fs = new FileStream(dir, FileMode.Create))
                {

                    fs.Flush();
                    fs.Write(bytes, 0, bytes.Length);
                }
                ms00.Dispose();

            }
            catch (System.Exception)
            {
                throw new System.Exception(TifImageErrorMessage.ERROR_APPEND_IMG);
            }
        }

        public MemoryStream ToStream()
        {
            var ret = new MemoryStream();
            ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
            var encoderParameters = GetEncoderParams();

            Bitmap firstImg = _imgs[0];
            firstImg.Save(ret, encoderInfo, encoderParameters);

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);

            for (int i = 1; i < _imgs.Count; i++)
            {
                firstImg.SaveAdd(_imgs[i], encoderParameters);
            }
            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            firstImg.SaveAdd(encoderParameters);

            var r = new MemoryStream();
            firstImg.Save(r, ImageFormat.Tiff);
            return r;
        }

        /// <summary>
        /// Appends a tiff image into the tiff object
        /// </summary>
        /// <param name="img">Tiff image to append</param>
        public void Append(TiffImage tifimg)
        {
            foreach (Bitmap bmpimg in tifimg._imgs)
            {
                _imgs.Add(bmpimg);
            }
        }

        void AppendBack(TiffImage tifimg,int index=-1)
        {
            
            List<Bitmap> __imgs = new List<Bitmap>(tifimg._imgs);
            

            if(index==-1 || index > _imgs.Count)
            {
                foreach (Bitmap bmpimg in _imgs)
                {
                    __imgs.Add(bmpimg);
                }
                _imgs = __imgs;
            }
            else if(index< _imgs.Count)
            {
                foreach (Bitmap bmpimg in _imgs)
                {
                    __imgs.Insert(index++, bmpimg);
                }
                _imgs = __imgs;
            }
        }

        /// <summary>
        /// Number of pages in the tiff image
        /// </summary>
        public int NumPages {
            get
            {
                return _imgs.Count;
            }
        }

        /// <summary>
        /// Splits the incoming tif stream into an array of bitmaps
        /// So users can add other pages then merge and save them later
        /// </summary>
        /// <param name="tifstream"></param>
        void SplitBitMaps(Stream tifstream)
        {
            Image sampleTiffImage = Image.FromStream(tifstream);
            int pageCount = sampleTiffImage.GetFrameCount(FrameDimension.Page);
            for (int pageNum = 0; pageNum < pageCount; pageNum++)
            {
                sampleTiffImage.SelectActiveFrame(FrameDimension.Page, pageNum);

                _imgs.Add((Bitmap)sampleTiffImage.Clone());
            }

        }

        EncoderParameters GetEncoderParams()
        {
            Encoder encoder = Encoder.SaveFlag;
            Encoder encComp = Encoder.Compression;
            EncoderParameters encoderParameters = new EncoderParameters(2);

            ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
            encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);
            encoderParameters.Param[1] = new EncoderParameter(encComp, (long)EncoderValue.CompressionLZW);
            return encoderParameters;
        }

        public List<byte[]> ToByteArrayList()
        {
            var ret = new List<byte[]>();

            foreach (var item in _imgs)
            {
                ret.Add(ImageToByte(item));
            }
            return ret;
        }

       byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        //remove image
        public static void RemovePage(int page, string dir)
        {
            TiffImage i;
            using (var fs = new FileStream(dir, FileMode.Open))
            {
                var ms = new MemoryStream();
                fs.CopyTo(ms);
                i = new TiffImage(ms);
                i._imgs.RemoveAt(page-1);
            }
            if (i._imgs.Count == 0)
                File.Delete(dir);
            else
                i.Save(dir);
        }
    }
}
