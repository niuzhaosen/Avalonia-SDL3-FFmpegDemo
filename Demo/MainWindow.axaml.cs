using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FFmpegTest;
using System;
using System.Threading.Tasks;

namespace Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            RtspAddress.Text = "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1";
            FFmpegInit.Init();
            this.PlayBtn.Click += PlayBtn_Click;
        }

        private void PlayBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            WriteableBitmap map2 = new WriteableBitmap(new PixelSize(1920, 1080), new Vector(96, 96),
                   PixelFormat.Bgra8888, AlphaFormat.Premul);
            Img2.Source = map2;
            WriteableBitmap map1 = new WriteableBitmap(new PixelSize(1920, 1080), new Vector(96, 96),
               PixelFormat.Bgra8888, AlphaFormat.Premul);
            Img1.Source = map1;
            string rtspAddress = RtspAddress.Text;
            Task.Run(() =>
            {             
                var locked2 = map2.Lock();
                IntPtr ptr2 = locked2.Address;
                locked2.Dispose();
                RtspClientTest rtspClient = new RtspClientTest(RePaint, ptr2, rtspAddress);
                rtspClient.PlayByRtspClient();
            });
           
            Task.Run(() =>
            {
                var locked2 = map1.Lock();
                IntPtr ptr2 = locked2.Address;
                locked2.Dispose();
                RtspClientTest rtspClient = new RtspClientTest(RePaint1, ptr2, rtspAddress);
                rtspClient.PlayByFFmpeg();
            });

        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            

        }
        public void RePaint()
        {

            Dispatcher.UIThread.InvokeAsync(() => Img2.InvalidateVisual());
          



        }
        public void RePaint1()
        {


            Dispatcher.UIThread.InvokeAsync(() => Img1.InvalidateVisual());



        }

    }
}