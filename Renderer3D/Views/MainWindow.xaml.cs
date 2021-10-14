using Renderer3D.Viewmodels;
using System.Windows;

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
            DataContext = new RendererViewmodel(this);
        }
    }
}
