using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Realestate_portal.HttpPostedFile
{
    public class MemoryPostedFile : HttpPostedFileBase
    {
        private readonly byte[] fileBytes;

        public MemoryPostedFile(byte[] fileBytes,string fileName)
        {
            this.fileBytes = fileBytes;
            this.FileName = fileName;
            this.InputStream = new MemoryStream(fileBytes);
        }

        public override int ContentLength => fileBytes.Length;

        public override string FileName { get; }

        public override Stream InputStream { get; }
    }
}