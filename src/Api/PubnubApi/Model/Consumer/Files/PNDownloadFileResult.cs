using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class PNDownloadFileResult
    {
        public PNDownloadFileResult() { }
        public PNDownloadFileResult(byte[] fileBytes, string fileName)
        {
            this.FileBytes = fileBytes;
            this.FileName = fileName;
        }
        public byte[] FileBytes { get; private set; }
        public string FileName { get; private set; }

        public bool SaveFileToLocal(string destinationFullFilePath)
        {
            bool ret = false;
            if (FileBytes == null)
            {
                return false;
            }
#if !NETSTANDARD10 && !NETSTANDARD11
            try
            {
                if (System.IO.Path.HasExtension(destinationFullFilePath))
                {
                    System.IO.File.WriteAllBytes(destinationFullFilePath, this.FileBytes);
                    ret = true;
                }
                else if (System.IO.Directory.Exists(destinationFullFilePath))
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(destinationFullFilePath, FileName), this.FileBytes);
                    ret = true;
                }
            }
            catch
            {
                ret = false;
            }
#endif

            return ret;
        }
    }
}
