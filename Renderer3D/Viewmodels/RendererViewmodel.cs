using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Render3D.Viewmodels.Commands;
using System.Threading;

namespace Renderer3D.Viewmodels
{
    public class RendererViewmodel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender,e) => { };

        public PixelFormat PixelFormat { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Stride => (Width * PixelFormat.BitsPerPixel + 7) / 8;
        public BitmapSource Frame { get; private set; }
      
        public ICommand MouseMoveCommand { get; }

        private WriteableBitmap _bitmap;

        public RendererViewmodel(Window window, PixelFormat pixelFormat)
        {
            //Init properties
            PixelFormat = pixelFormat;
            Width = (int)window.Width;
            Height = (int)window.Height;

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Width = (int)e.NewSize.Width;
                Height = (int)e.NewSize.Height;
            };

            //Mouse drag handler
            MouseMoveCommand = new RelayCommand<MouseEventArgs>((args) =>
            {
                Task.Factory.StartNew(Render, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }, (args) => args.LeftButton == MouseButtonState.Pressed);

            //Initial render
            Render();
        }

        private void Render()
        {
            //Init bitmap
            _bitmap = new WriteableBitmap(BitmapSource.Create(Width, Height, 96d, 96d, PixelFormat, null, new byte[Height * Stride], Stride));
            
            //Draw logic here...
            //Use _bitmap.ExtensionMethod to draw




            //Update UI
            _bitmap.Freeze();
            Frame = _bitmap;
        }
    }
}
