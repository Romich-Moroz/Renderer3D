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
using System.Windows.Media.Imaging;

namespace Renderer3D.Viewmodels
{
    public class RendererViewmodel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        private Scene Scene { get; }

        public ICommand MouseMoveCommand { get; }
        public ICommand MouseWheelCommand { get; }
        public ICommand KeyDownCommand { get; }
        public ICommand OpenModelCommand { get; }

        public BitmapSource Frame { get; private set; }
        public Point PreviousMousePosition { get; set; }
        public long RenderTime { get; set; }

        private readonly List<Mesh> Meshes = new List<Mesh>
        {
            MeshParser.Parse("../../../RenderModels/DebugCube/cube.obj"),
            MeshParser.Parse("../../../RenderModels/Wooden Crate/Wooden Crate.obj"),
            MeshParser.Parse("../../../RenderModels/Skull/12140_Skull_v3_L2.obj"),
            MeshParser.Parse("../../../RenderModels/RC_Car/RC_Car.obj"),
            MeshParser.Parse("../../../RenderModels/Eye/eyeball.obj"),
            MeshParser.Parse("../../../RenderModels/Cat/12221_Cat_v1_l3.obj"),
            MeshParser.Parse("../../../RenderModels/AfricanHead/african_head.obj"),
        };

        private void UpdateFrame()
        {
            Frame = Scene.GetRenderedScene(out long rt);
            RenderTime = rt;
        }

        public RendererViewmodel(Window window)
        {
            Scene = new Scene((int)window.Width, (int)window.Height, Meshes[0]);

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Scene.ResizeScene((int)e.NewSize.Width, (int)e.NewSize.Height - 30);

                UpdateFrame();
            };

            //Mouse drag handler
            MouseMoveCommand = new RelayCommand<MouseEventArgs>((args) =>
            {
                if (PreviousMousePosition.X == -1)
                {
                    PreviousMousePosition = Mouse.GetPosition(Application.Current.MainWindow);
                }
                else
                {
                    Point currentPos = Mouse.GetPosition(Application.Current.MainWindow);
                    double y = currentPos.X - PreviousMousePosition.X;
                    double z = PreviousMousePosition.Y - currentPos.Y;
                    Scene.RotateModel(new Vector3(0, (float)y * Scene.SceneProperties.RenderProperties.Sensitivity, (float)z * Scene.SceneProperties.RenderProperties.Sensitivity));
                    PreviousMousePosition = currentPos;
                }
                UpdateFrame();
            }, (args) =>
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                {
                    return true;
                }

                PreviousMousePosition = new Point { X = -1, Y = -1 };
                return false;
            });

            //Mouse wheel handler
            MouseWheelCommand = new RelayCommand<MouseWheelEventArgs>((args) =>
            {
                int mult = args.Delta > 0 ? -1 : 1;
                Scene.OffsetCamera(new Vector3
                {
                    X = mult * Scene.SceneProperties.RenderProperties.ScaleStep,
                    Y = mult * Scene.SceneProperties.RenderProperties.ScaleStep,
                    Z = mult * Scene.SceneProperties.RenderProperties.ScaleStep
                });
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

                if (args.Key == Key.F)
                {
                    Scene.SceneProperties.RenderProperties.ShadingMode = Scene.SceneProperties.RenderProperties.ShadingMode == ShadingMode.Flat ? ShadingMode.None : ShadingMode.Flat;
                }

                if (args.Key == Key.P)
                {
                    Scene.SceneProperties.RenderProperties.ShadingMode = Scene.SceneProperties.RenderProperties.ShadingMode == ShadingMode.Phong ? ShadingMode.None : ShadingMode.Phong;
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
            Frame = Scene.GetRenderedScene(out long rt);
            RenderTime = rt;
        }
    }
}
