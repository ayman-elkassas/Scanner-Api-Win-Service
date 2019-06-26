using ClientScanner.TiffImage;
using scanner_win_service.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WIA;

namespace scanner_win_service.ScannerDriver
{

    /// <summary>
    /// Abstracts accessing client side scanner devices at the API
    /// </summary>
    public static class ScannerDriver
    {
        /// <summary>
        /// Invokes the scan command for the selected scanner and returns the image as a memory
        /// stream
        /// </summary>
        /// <param name="scanner_uri">The client side scanner unique identifier</param>
        /// <returns>Stream representing the image in TIFF format</returns>
        public static List<byte[]> GetScannedImage(string scanner_uri,int type,int threshold)
        {
            var selected_scanner = GetDesiredScanner(scanner_uri).Connect();

            //SetDeviceIntProperty(ref selected_scanner, 6146, 4);
            
            var scannerItem = selected_scanner.Items[1];
            // greyscale instead of black and white
            SetWIAProperty(scannerItem.Properties, "6146", type);

            SetWIAProperty(scannerItem.Properties, "6159", threshold);


            try
            {
                // Image resolution to fit A4 size
                // SET DPI to 150
                SetWIAProperty(scannerItem.Properties, "6147", 150);
                SetWIAProperty(scannerItem.Properties, "6148", 150);

                // Set paper A4 resolution to be 
                SetWIAProperty(scannerItem.Properties, "6151", 1754);
                SetWIAProperty(scannerItem.Properties, "6152", 1240);
            }
            catch (Exception)
            {
                // TODO:log error
            }


            var ret = new List<byte[]>();
            var imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatPNG);
            var buffer = (byte[])imageFile.FileData.get_BinaryData();
            buffer = RotateIfHorizontal(buffer);
            if (type == 4)
            {
                ret.Add(convertToBlackAndWhite(buffer));
            }
            else
            {
                ret.Add(buffer);
            }
            return ret;
        }

        /// <summary>
        /// Performs scan from the scanner ADF and produces image on both sides
        /// </summary>
        /// <param name="scanner_uri"></param>
        /// <returns></returns>
        public static List<byte[]> GetScannedImageADF(string scanner_uri, bool duplex,int type,int threshold)
        {
            var selected_scanner = GetDesiredScanner(scanner_uri).Connect();
            var scannerItem = selected_scanner.Items[1];

            SetDeviceIntProperty(ref selected_scanner, 3096, 1);


            if (duplex)
            {
                // set the scanner to get papers from ADF on both sides
                SetDeviceIntProperty(ref selected_scanner, 3088, 5);
            }
            else
            {
                // set the scanner to get papers from ADF on one side
                SetDeviceIntProperty(ref selected_scanner, 3088, 1);
            }

         
            // greyscale instead of black and white
            SetWIAProperty(scannerItem.Properties, "6146", type);
            SetWIAProperty(scannerItem.Properties, "6159", threshold);
            try
            {
                // SET DPI to 150
                SetWIAProperty(scannerItem.Properties, "6147", 150);
                SetWIAProperty(scannerItem.Properties, "6148", 150);

                // Set paper A4 resolution to be 
                SetWIAProperty(scannerItem.Properties, "6151", 1754);
                SetWIAProperty(scannerItem.Properties, "6152", 1240);
            }
            catch (Exception)
            {

            }

            System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.SaveFlag;
            System.Drawing.Imaging.Encoder encComp = System.Drawing.Imaging.Encoder.Compression;
            EncoderParameters encoderParameters = new EncoderParameters(2);

            ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders().First(i => i.MimeType == "image/tiff");
            encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);
            
            
            encoderParameters.Param[1] = new EncoderParameter(encComp, (long)EncoderValue.CompressionLZW);

            
            var front = ImageFileToStream((ImageFile)scannerItem.Transfer(FormatID.wiaFormatPNG));
            var tifImg = new TiffImage(front);

            while (true)
            {
                try
                {
                    var imgfrnt = new TiffImage(ImageFileToStream((ImageFile)scannerItem.Transfer(FormatID.wiaFormatPNG)));
                    tifImg.Append(imgfrnt);
                }
                catch (Exception)
                {

                    break;
                }
            }
            var ret = new List<byte[]>();
            foreach (var item in tifImg.ToByteArrayList())
            {
                var rotated_img = RotateIfHorizontal(item);
                if (type == 4)
                {
                    ret.Add(convertToBlackAndWhite(rotated_img));
                }
                else
                {
                    ret.Add(rotated_img);
                }
            }
            return ret;
        }

        /// <summary>
        /// Return a list of the available scanners uri
        /// </summary>
        /// <returns>List of string for the scanners URI available</returns>
        public static List<string> GetScanners()
        {
            List<string> ret = new List<string>();
            var deviceManager = new DeviceManager();
            foreach (DeviceInfo item in deviceManager.DeviceInfos)
            {
                ret.Add(GetScannerURI(item));
            }
            return ret;
        }


        /// <summary>
        /// Selects a specific scanner from the client available scanners 
        /// </summary>
        /// <param name="query_scanner_uri">selected scanner uri</param>
        /// <returns></returns>
        static DeviceInfo GetDesiredScanner(string query_scanner_uri)
        {
            var deviceManager = new DeviceManager();
            foreach (DeviceInfo scanner in deviceManager.DeviceInfos)
            {
                if (GetScannerURI(scanner) == query_scanner_uri) return scanner;
            }
            throw new Exception();
            
        }

        /// <summary>
        /// Returns the scanner unique identifier URI for a given scanner object
        /// </summary>
        /// <param name="scanner"></param>
        /// <returns></returns>
        static string GetScannerURI(DeviceInfo scanner)
        {
            var scanner_name = scanner.Properties["Name"].get_Value().ToString();
            var scanner_port  = scanner.Properties["Port"].get_Value().ToString();
            var scanner_uri = (scanner_name + scanner_port).Replace(" ", "");
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            scanner_uri = rgx.Replace(scanner_uri, "");
            return scanner_uri;
        }


        /// <summary>
        /// Sets the specific WIA device properties. Used to enable switching between
        /// flatbed scanning and ADF
        /// </summary>
        /// <param name="device"></param>
        /// <param name="propertyID"></param>
        /// <param name="propertyValue"></param>
        private static void SetDeviceIntProperty(ref Device device, int propertyID, int propertyValue)
        {
            foreach (Property p in device.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    object value = propertyValue;
                    p.set_Value(ref value);
                    break;
                }
            }
        }

        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }


        private static Image ImageFileToImage(ImageFile src)
        {
            var imageBytes = (byte[])src.FileData.get_BinaryData();
            var ms = new MemoryStream(imageBytes);
            return Image.FromStream(ms);
        }

        private static MemoryStream ImageFileToStream(ImageFile src)
        {
            var imageBytes = (byte[])src.FileData.get_BinaryData();
            return new MemoryStream(imageBytes);
        }

        /// <summary>
        /// Applies few image processing algorithms to convert a grayscale or rgb images
        /// into clear black and white for less storage requirements.
        /// Notice also the method will auto rotate images aquired from different scanners
        /// type in a way that the orientation is always the same
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static byte[] convertToBlackAndWhite(byte[] img)
        {
            var rgbimg = Converter.ConvertToRGB((Bitmap)Image.FromStream(new MemoryStream(img)));
            var bwimg = Converter.ConvertToBitonal(rgbimg);
            //if (bwimg.Size.Width > bwimg.Size.Height)
            //    bwimg.RotateFlip(RotateFlipType.Rotate90FlipNone);
            using (var stream = new MemoryStream())
            {
                bwimg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        static byte[] RotateIfHorizontal(byte[] img)
        {
            var ms = new MemoryStream(img);
            var _img = new Bitmap(ms);
            if (_img.Size.Width > _img.Size.Height)
                _img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            var ret_ms = new MemoryStream();
            _img.Save(ret_ms, ImageFormat.Png);
            return ret_ms.ToArray();
        }

    }
}
