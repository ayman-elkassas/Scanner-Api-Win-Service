using ClientScanner.TiffImage;
using scanner_win_service.ScannerDriver;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace scanner_win_service.Controller
{
    public class ScannerController : ApiController
    {
        /// <summary>
        /// Returns the list of available scanners on the client side
        /// Addressable via /api/scanner
        /// </summary>
        /// <returns></returns>
        public IEnumerable Get()
        {
            return ScannerDriver.ScannerDriver.GetScanners();
        }


        /// <summary>
        /// Returns the image(s) captured from the scanner
        /// </summary>
        /// <param name="scanner_uri">scanner unique identifier</param>
        /// <param name="use_adf">bool to allow taking images from auto document feed</param>
        /// <param name="use_duplex">to use duplex scanning if available (some scanners allow this with adf)</param>
        /// <returns></returns>
        public IEnumerable<byte[]> Get(string scanner_uri, bool use_adf, bool use_duplex, int type,int threshold)
        {
            var ret = new List<byte[]>();
            if (use_adf)
            {
                return ScannerDriver.ScannerDriver.GetScannedImageADF(scanner_uri, use_duplex,type, threshold);
            }
            else
            {
                return ScannerDriver.ScannerDriver.GetScannedImage(scanner_uri,type, threshold);
            }
        }
    }

}
