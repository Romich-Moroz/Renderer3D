using Microsoft.Win32;
using Renderer3D.Models.Data;
using Renderer3D.Models.Parser;
using Renderer3D.Models.Scene;
using Renderer3D.Viewmodels.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
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
        public ICommand MouseWheelCommand { get; }
        public ICommand KeyDownCommand { get; }
        public ICommand OpenModelCommand { get; }

        public Point PreviousPosition { get; set; }
        public float Sensitivity { get; set; } = (float)System.Math.PI / 360;
        public float MoveStep { get; set; } = 0.25f;
        public float ScaleStep { get; set; } = 1f;

        private Scene Scene { get; }

        private readonly List<Mesh> Meshes = new List<Mesh>
        {
            MeshParser.Parse("../../../RenderModels/Debug/debug.obj"),
            MeshParser.Parse("../../../RenderModels/Skull/12140_Skull_v3_L2.obj"),
            MeshParser.Parse("../../../RenderModels/Custom/car/uploads_files_2792345_Koenigsegg.obj"),
            MeshParser.Parse("../../../RenderModels/Custom/RC_Car/RC_Car.obj"),
            MeshParser.Parse("../../../RenderModels/Custom/Boots.obj"),
            MeshParser.Parse("../../../RenderModels/Custom/Eye/eyeball.obj"),
            MeshParser.Parse("../../../RenderModels/Custom/Head/head.obj"),
        };

        private void UpdateFrame()
        {
            Frame = Scene.GetRenderedScene();
        }

        public RendererViewmodel(Window window, PixelFormat pixelFormat)
        {
            Scene = new Scene(pixelFormat, (int)window.Width, (int)window.Height, Meshes[0]);

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Scene.ResizeScene((int)e.NewSize.Width, (int)e.NewSize.Height - 30);

                UpdateFrame();
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
                    Point currentPos = Mouse.GetPosition(Application.Current.MainWindow);
                    double y = currentPos.X - PreviousPosition.X;
                    double z = PreviousPosition.Y - currentPos.Y;
                    Scene.RotateModel(new Vector3(0, (float)y * Sensitivity, (float)z * Sensitivity));
                    PreviousPosition = currentPos;
                }
                UpdateFrame();
            }, (args) =>
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                {
                    return true;
                }

                PreviousPosition = new Point { X = -1, Y = -1 };
                return false;
            });

            //Mouse wheel handler
            MouseWheelCommand = new RelayCommand<MouseWheelEventArgs>((args) =>
            {
                int mult = args.Delta > 0 ? -1 : 1;
                Scene.OffsetCamera(new Vector3 { X = mult * ScaleStep, Y = mult * ScaleStep, Z = mult * ScaleStep });
                UpdateFrame();
            }, null);

            //Key down handler
            KeyDownCommand = new RelayCommand<KeyEventArgs>((args) =>
            {
                int offsetX = args.Key == Key.A ? -1 : args.Key == Key.D ? 1 : 0;
                int offsetY = args.Key == Key.W ? 1 : args.Key == Key.S ? -1 : 0;
                int offsetZ = args.Key == Key.Q ? 1 : args.Key == Key.E ? -1 : 0;
                Scene.OffsetModel(new Vector3(offsetX, offsetY, offsetZ));

                if (args.Key == Key.R)
                {
                    Scene.ResetState();
                }

                if (args.Key == Key.T)
                {
                    Scene.ToggleTriangleMode();
                }

                if (args.Key >= Key.D0 && args.Key <= Key.D9)
                {
                    int index = args.Key - Key.D0;
                    if (index < Meshes.Count)
                    {
                        Scene.ChangeMesh(Meshes[index]);
                    };
                }

                UpdateFrame();
            }, null);

            //Main menu button handler
            OpenModelCommand = new RelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    Scene.ChangeMesh(MeshParser.Parse(openFileDialog.FileName));
                }
                UpdateFrame();
            }, null);

            //Initial render
            Frame = Scene.GetRenderedScene();
        }
    }
}
