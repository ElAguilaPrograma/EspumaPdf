using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Espuma.Application.Services
{
    public interface InterfaceIsPdf
    {
        bool ValidatePdf(Stream stream);
    }
    public class IsPdf : InterfaceIsPdf
    {
        public bool ValidatePdf(Stream stream)
        {
            var buffer = new byte[5];
            var originalPosition = stream.Position;

            stream.Position = 0;
            stream.Read(buffer, 0, buffer.Length);
            stream.Position = originalPosition;

            var header = System.Text.Encoding.ASCII.GetString(buffer);
            return header == "%PDF-";
        }
    }
}
