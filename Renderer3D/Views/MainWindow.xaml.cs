using Renderer3D.Viewmodels;
using System.Windows;
using System.Windows.Media;

namespace Renderer3D.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new RendererViewmodel(this, PixelFormats.Rgba64);
        }
    }
}
