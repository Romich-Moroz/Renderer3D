using Renderer3D.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Models.Renderer
{
    public class Renderer
    {
        public PixelFormat PixelFormat { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;
        public ObjectModel ObjectModel { get; set;}

        public Renderer(PixelFormat pixelFormat, int width, int height, ObjectModel model) => (PixelFormat, Width, Height, ObjectModel) = (pixelFormat, width, height, model);

        public BitmapSource Render()
        {
            //Init bitmap
            WriteableBitmap _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));

            //Draw logic here...
            //Use _bitmap.ExtensionMethod to draw




            _bitmap.Freeze();
            return _bitmap;
        }
    }
}
