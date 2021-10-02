﻿using Microsoft.Win32;
using Renderer3D.Models.Data;
using Renderer3D.Models.Parser;
using Renderer3D.Models.Renderer;
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

        private Renderer Renderer { get; }

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
            Frame = Renderer.Render();
        }

        public RendererViewmodel(Window window, PixelFormat pixelFormat)
        {
            Renderer = new Renderer(pixelFormat, (int)window.Width, (int)window.Height, Meshes[0]);

            //Window resize handler
            window.SizeChanged += (sender, e) =>
            {
                Renderer.Width = (int)e.NewSize.Width;
                Renderer.Height = (int)e.NewSize.Height - 30;
                Renderer.UpdateWritableBitmap();

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
                    double x = currentPos.X - PreviousPosition.X;
                    double y = currentPos.Y - PreviousPosition.Y;
                    Renderer.ModelRotationX += (float)y * Sensitivity;
                    Renderer.ModelRotationY += (float)x * Sensitivity;
                    //Renderer.RotateCameraY((float)x * Sensitivity);
                    //Renderer.RotateCameraX((float)y * Sensitivity);
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
                Renderer.OffsetCamera(new Vector3 { X = mult * ScaleStep, Y = mult * ScaleStep, Z = mult * ScaleStep });
                UpdateFrame();
            }, null);

            //Key down handler
            KeyDownCommand = new RelayCommand<KeyEventArgs>((args) =>
            {
                if (args.Key == Key.A)
                {
                    Renderer.Offset += new Vector3 { X = -1, Y = 0, Z = 0 } * MoveStep;
                }

                if (args.Key == Key.D)
                {
                    Renderer.Offset += new Vector3 { X = 1, Y = 0, Z = 0 } * MoveStep;
                }

                if (args.Key == Key.W)
                {
                    Renderer.Offset += new Vector3 { X = 0, Y = 1, Z = 0 } * MoveStep;
                }

                if (args.Key == Key.S)
                {
                    Renderer.Offset += new Vector3 { X = 0, Y = -1, Z = 0 } * MoveStep;
                }

                if (args.Key == Key.Q)
                {
                    Renderer.Offset += new Vector3 { X = 0, Y = 0, Z = 1 } * MoveStep;
                }

                if (args.Key == Key.E)
                {
                    Renderer.Offset += new Vector3 { X = 0, Y = 0, Z = -1 } * MoveStep;
                }

                if (args.Key == Key.R)
                {
                    Renderer.ResetState();
                }

                if (args.Key == Key.T)
                {
                    Renderer.TriangleMode ^= true;
                }

                if (args.Key >= Key.D0 && args.Key <= Key.D9)
                {
                    int index = args.Key - Key.D0;
                    if (index < Meshes.Count)
                    {
                        Renderer.ChangeMesh(Meshes[index]);
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
                    Renderer.ChangeMesh(MeshParser.Parse(openFileDialog.FileName));
                }
                UpdateFrame();
            }, null);

            //Initial render
            Frame = Renderer.Render();
        }
    }
}
