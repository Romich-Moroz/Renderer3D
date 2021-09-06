using Renderer3D.Viewmodels.Commands;
using Renderer3D.Models.Parser;
using Renderer3D.Models.Renderer;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace Renderer3D.Viewmodels
{
    public class RendererViewmodel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public BitmapSource Frame { get; private set; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseWheelCommand { get; }
        public ICommand KeyDownCommand { get; }
        public Point PreviousPosition { get; set;}
        public float Sensitivity { get; set;} = (float)System.Math.PI/360;
        public float MoveStep { get; set; } = 0.25f;
        public float ScaleStep { get; set; } = 0.05f;

        private Renderer Renderer { get; }

        public RendererViewmodel(Window window, PixelFormat pixelFormat)
        {
            //Init renderer (for test purposes change your model name here)
            //ObjectModel objectModel = ObjectModelParser.Parse("../../../RenderModels/Skull/12140_Skull_v3_L2.obj");
            //ObjectModel objectModel = ObjectModelParser.Parse("../../../RenderModels/Custom/Klesk/klesk.obj");
            //ObjectModel objectModel = ObjectModelParser.Parse("../../../RenderModels/Custom/bugatti/bugatti.obj");
            ObjectModel objectModel = ObjectModelParser.Parse("../../../RenderModels/Custom/Head/head.obj");
            Renderer = new Renderer(pixelFormat, (int)window.Width, (int)window.Height, objectModel);

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Renderer.Width = (int)e.NewSize.Width;
                Renderer.Height = (int)e.NewSize.Height - 30;
                Task.Factory.StartNew(() => { Frame = Renderer.Render(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            };

            //Mouse drag handler
            MouseMoveCommand = new RelayCommand<MouseEventArgs>((args) =>
            {
                if (PreviousPosition.X == -1)
                {
                    PreviousPosition = Mouse.GetPosition(Application.Current.MainWindow);
                }
                else
                {
                    var currentPos = Mouse.GetPosition(Application.Current.MainWindow);
                    var x = currentPos.X - PreviousPosition.X;
                    var y = currentPos.Y - PreviousPosition.Y;
                    Renderer.RotationY += (float)x*Sensitivity;
                    Renderer.RotationX += (float)y*Sensitivity;
                    PreviousPosition = currentPos;
                }
                
                Task.Factory.StartNew(() => { Frame = Renderer.Render(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }, (args) =>  
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                    return true;
                PreviousPosition = new Point { X = -1, Y = -1 };
                return false;
            });

            MouseWheelCommand = new RelayCommand<MouseWheelEventArgs>((args) => 
            {
                if (args.Delta > 0)
                {
                    Renderer.Scale += new Vector3 { X = ScaleStep, Y = ScaleStep, Z = ScaleStep };
                }
                else
                {
                    Renderer.Scale -= new Vector3 { X = ScaleStep, Y = ScaleStep, Z = ScaleStep };
                }
                Task.Factory.StartNew(() => { Frame = Renderer.Render(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            }, null);

            KeyDownCommand = new RelayCommand<KeyEventArgs>((args) => 
            {
                var moveKeyPressed = args.Key == Key.A || args.Key == Key.W || args.Key == Key.S || args.Key == Key.D || args.Key == Key.Q || args.Key == Key.E;
                if (args.Key == Key.A)
                    Renderer.Offset += new Vector3 { X = -1, Y = 0, Z = 0 } * MoveStep;
                if (args.Key == Key.D)
                    Renderer.Offset += new Vector3 { X = 1, Y = 0, Z = 0 } * MoveStep;
                if (args.Key == Key.W)
                    Renderer.Offset += new Vector3 { X = 0, Y = 1, Z = 0 } * MoveStep;
                if (args.Key == Key.S)
                    Renderer.Offset += new Vector3 { X = 0, Y = -1, Z = 0 } * MoveStep;
                if (args.Key == Key.Q)
                    Renderer.Offset += new Vector3 { X = 0, Y = 0, Z = 1 } * MoveStep;
                if (args.Key == Key.E)
                    Renderer.Offset += new Vector3 { X = 0, Y = 0, Z = -1 } * MoveStep;
                if (moveKeyPressed)
                    Task.Factory.StartNew(() => { Frame = Renderer.Render(); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            },null);

            //Initial render
            Frame = Renderer.Render();
        }


    }
}
