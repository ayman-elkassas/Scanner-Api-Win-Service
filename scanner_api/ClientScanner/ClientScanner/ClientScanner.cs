using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ClientScanner.TiffImage;

namespace ClientScanner.ClientScanner
{
    static class ScannerErrorMessages
    {
        public static string ERR_CONNECTION_MSG =  "من فضلك تأكد من توصيل ال scanner";
        public static string ERR_DURING_SCAN = "خطأ اثناء ال scan برجاء اعادة المحاولة";

    }

    /// <summary>
    /// Abstracts accessing the scanner on client side
    /// </summary>
    public class ClientScanner : IClientScanner
    {
        HttpClient _client;
        string _clientIp;

        /// <summary>
        /// Creates a client scanner object given API ip address and port number
        /// </summary>
        /// <param name="clientIp">The ip address running the API</param>
        /// <param name="apiPort">API listining port</param>
        public ClientScanner(string clientIp, int apiPort=3001)
        {
            _clientIp = clientIp;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(string.Format("http://{0}:{1}/", _clientIp, apiPort.ToString()));
        }

        /// <summary>
        /// Returns the URI for each scanner connected to the client device
        /// </summary>
        /// <returns></returns>
        public List<string> GetScanners()
        {
            try
            {
                HttpResponseMessage response = _client.GetAsync("api/scanner").Result;
                var ret = response.Content.ReadAsAsync<List<string>>().Result;
                return ret;
            }
            catch (Exception)
            {
                throw new Exception(ScannerErrorMessages.ERR_CONNECTION_MSG);
            }
           
        }

        /// <summary>
        /// Performs the scan on the first available scanner 
        /// and returns the scanned image as TIFF
        /// </summary>
        /// <returns></returns>
        public TiffImage.TiffImage Scan(bool use_adf, bool use_duplex,int type,int threshold)
        {
            string firstScannerUri;
            try
            {
                firstScannerUri = GetScanners()[0];
            }
            catch (Exception)
            {
                throw new Exception(ScannerErrorMessages.ERR_CONNECTION_MSG);
            }
            return Scan(firstScannerUri, use_adf, use_duplex,type, threshold);
        }


        /// <summary>
        /// Performs the scan on the given scanner URI and returns the scanned image as TIFF
        /// </summary>
        /// <param name="scanner_uri">scanner URI to perform scan with</param>
        /// <returns></returns>
        public TiffImage.TiffImage Scan(string scanner_uri, bool use_adf, bool use_duplex,int type,int threshold)
        {
            try
            {

                HttpResponseMessage response = _client.GetAsync(
                    string.Format("api/scanner?scanner_uri={0}&use_adf={1}&use_duplex={2}&type={3}&threshold={4}", scanner_uri, use_adf, use_duplex,type,threshold)).Result;

            var captured_image = response.Content.ReadAsAsync<List<byte[]>>().Result;          
            var ret = new List<TiffImage.TiffImage>();

            foreach (var item in captured_image)
            {
                ret.Add(new TiffImage.TiffImage(new MemoryStream(item)));
            }

            for (int i = 1; i < ret.Count; i++)
            {
                ret[0].Append(ret[i]);
            }
            return ret[0];
            }
            catch (Exception)
            {
                throw new Exception(ScannerErrorMessages.ERR_DURING_SCAN);
            }
        }

    }
}
