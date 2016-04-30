using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TriToe.EncodeBase64
{
    public class Encoder
    {
        public string ConvertToBase64(string filePath)
        {
            using (Image image = Image.FromFile(filePath))
            {
                var format = image.RawFormat;
                return ConvertToBase64(image, format);
            }
        }

        public string ConvertToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //First Convert Image to byte[]
                image.Save(ms, format);
                // Check that image is less than 75k. It's a bad idea to base64 encode big images.
                if (ms.Length <= 76800)
                {
                    byte[] imageBytes = ms.ToArray();

                    //Get Image MimeType
                    var codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
                    var mimeType = codec.MimeType;

                    //Then Convert byte[] to Base64 String
                    string base64String = string.Format("data:{0};base64,{1}", mimeType, Convert.ToBase64String(imageBytes));
                    return base64String;
                }
            }
            return string.Empty;
        }
    }
}
