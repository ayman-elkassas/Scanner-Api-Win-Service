using System;
using System.Collections.Generic;

namespace ClientScanner.ClientScanner
{

    /// <summary>
    /// Interface for any object exposing the client scanner
    /// </summary>
    public interface IClientScanner
    {
        List<string> GetScanners();
        TiffImage.TiffImage Scan(bool use_adf, bool use_duplex,int type, int threshold);
        TiffImage.TiffImage Scan(string scanner_uri, bool use_adf, bool use_duplex,int type, int threshold);
    }
}
