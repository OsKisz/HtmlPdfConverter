using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlPdfConverter
{
    public class HtmlPdfConverter_Exceptions : Exception
    {
        public HtmlPdfConverter_Exceptions(string message) : base(message)
        {
            Console.WriteLine($"HtmlPdfConverter_Exception: {message}");
        }

        public HtmlPdfConverter_Exceptions(string message, Exception innerException) : base(message, innerException)
        {
            Console.WriteLine($"HtmlPdfConverter_Exception: {message}, Inner Exception: {innerException.Message}");
        }
    }
}
