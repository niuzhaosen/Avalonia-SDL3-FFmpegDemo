using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FFmpegTest;
using System;

namespace Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(new PixelSize(1920, 1080), new Vector(96, 96),
                PixelFormat.Bgra8888, AlphaFormat.Premul);
            img.Source = writeableBitmap;
            FFmpegInit.Init();
            var xx = writeableBitmap.Lock();
            IntPtr xxx = xx.Address;
            xx.Dispose();
            RtspClientTest rtspClient = new RtspClientTest(Run, xxx);
            rtspClient.Run();
          
        }
        public void Run(byte[] data)
        {
            img.InvalidateVisual();



        }

    }
}