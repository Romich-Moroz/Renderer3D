using Render3D.Viewmodels.Commands;
using Renderer3D.Models.Parser;
using Renderer3D.Models.Renderer;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Renderer3D.Viewmodels
{
    public class RendererViewmodel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public BitmapSource Frame { get; private set; }
        public ICommand MouseMoveCommand { get; }

        private Renderer Renderer { get; }

        public RendererViewmodel(Window window, PixelFormat pixelFormat)
        {
            //Init renderer (for test purposes change your model name here)
            ObjectModel objectModel = ObjectModelParser.Parse("../../../RenderModels/Skull/12140_Skull_v3_L2.obj");
            Renderer = new Renderer(pixelFormat, (int)window.Width, (int)window.Height, objectModel);

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Renderer.Width = (int)e.NewSize.Width;
                Renderer.Height = (int)e.NewSize.Height;
            };

            //Mouse drag handler
            MouseMoveCommand = new RelayCommand<MouseEventArgs>((args) =>
            {
                Task.Factory.StartNew(() => { Frame = Renderer.Render(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }, (args) => args.LeftButton == MouseButtonState.Pressed);

            //Initial render
            Frame = Renderer.Render();
        }


    }
}
