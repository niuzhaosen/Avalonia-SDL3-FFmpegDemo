using Avalonia;
using Avalonia.Controls;

using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FFmpegTest;
using System;
using System.Threading.Tasks;
using Silk.NET.SDL;
using System.Threading;
using System.Runtime.InteropServices;
using Avalonia.Rendering;
using Window = Silk.NET.SDL.Window;
namespace Demo
{
    public unsafe partial class MainWindow : Avalonia.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            FFmpegInit.Init();

        }


        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //SDl播放
            Sdl sdl = Sdl.GetApi();
          int z=  sdl.Init(Sdl.InitVideo);
          
            Window* Window =  sdl.CreateWindowFrom((void*)dis.Handle);
       
            Renderer* Renderer = sdl.CreateRenderer(Window, -1, (uint)RendererFlags.Accelerated);

         
            //原生绘制
            WriteableBitmap writeableBitmap = new WriteableBitmap(new PixelSize(1920, 1080), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888);
            img.Source=writeableBitmap;
            var locked = writeableBitmap.Lock();
            IntPtr ptr = locked.Address;
            locked.Dispose();


            RtspClientTest rtspClientTest = new RtspClientTest("rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1", nint.Zero,Renderer,sdl,RePaint, ptr);
            System.Threading.Thread thread = new System.Threading.Thread(rtspClientTest.PlayByFFmpeg);
            thread.IsBackground= true;
            thread.Start();
        }
        public void RePaint()
        {

            Dispatcher.UIThread.InvokeAsync(() => img.InvalidateVisual());




        }

    }
    public class NativeEmbeddingControl : NativeControlHost
    {
        public IntPtr Handle { get; private set; }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            var handle = base.CreateNativeControlCore(parent);
            Handle = handle.Handle;
            Console.WriteLine($"Handle : {Handle}");
            return handle;
        }
    }
}