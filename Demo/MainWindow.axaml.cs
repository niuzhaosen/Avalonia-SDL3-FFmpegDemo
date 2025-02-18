
using Avalonia.Controls;


using Avalonia.Platform;

using System;
using SDL3;
using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;
using System.IO;
using FFmpeg.AutoGen.Abstractions;
using static SDL3.SDL;
using System.Threading;

namespace Demo
{
    public unsafe partial class MainWindow : Avalonia.Controls.Window
    {
        public void Init()
        {
            SDL.Init(SDL.InitFlags.Video);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string current = Environment.CurrentDirectory;
                string probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");

                while (current != null)
                {
                    var ffmpegBinaryPath = Path.Combine(current, probe);

                    if (Directory.Exists(ffmpegBinaryPath))
                    {
                        Console.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                        DynamicallyLoadedBindings.LibrariesPath = ffmpegBinaryPath;
                        break;
                    }

                    current = Directory.GetParent(current)?.FullName;
                }
            }
            else
            {

                DynamicallyLoadedBindings.LibrariesPath = "/data/ffmpeg/lib";
            }


            DynamicallyLoadedBindings.Initialize();
            ThreadQueue.Start();
        }
        public MainWindow()
        {
            InitializeComponent();
            Init();
            this.Loaded += MainWindow_Loaded;



        }
        nint window;
        nint render;
        nint targetTexture;
        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            uint prop = SDL.CreateProperties();
            SDL.SetPointerProperty(prop, SDL.Props.WindowCreateWin32HWNDPointer, dis.Handle);
            window = SDL.CreateWindowWithProperties(prop);
            render = SDL.CreateRenderer(window, null);

            ThreadQueue.AddTask(() =>
            {
                targetTexture = SDL.CreateTexture(render, SDL.PixelFormat.BGRA8888, TextureAccess.Target, (int)dis.Bounds.Width, (int)dis.Bounds.Height);
                SDL.SetRenderTarget(render, targetTexture);
                //borderColor
                SDL.SetRenderDrawColor(render, 0, 255, 0, 255);
                SDL.RenderClear(render);
                SDL.SetRenderTarget(render, nint.Zero);
                SDL.RenderTexture(render, targetTexture, nint.Zero, nint.Zero);
                SDL.RenderPresent(render);
                SDL.SetRenderTarget(render, targetTexture);
            });

            int w = (int)dis.Bounds.Width;
            int h = (int)dis.Bounds.Height;
            int borderSize = 3;
            for (int i = 0; i < 4; i++)
            {
                Thread thread=null;
                if (i == 0)
                    thread = new Thread(() =>
                    {
                        Decode(
                            "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1",
                            0+borderSize, 0+borderSize, w/2-(borderSize*2), h/2-(borderSize * 2)

                            );


                    })
                    { IsBackground = true };
                if (i == 1)
                    thread = new Thread(() =>
                    {
                        Decode(
                            "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1",
                          w/2,0 + borderSize, w / 2-borderSize, h / 2 - (borderSize * 2)

                            );


                    })
                    { IsBackground = true };
                if (i == 2)
                    thread = new Thread(() =>
                    {
                        Decode(
                            "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1",
                             0 + borderSize,h/2, w / 2 - (borderSize * 2), h / 2-borderSize

                            );


                    })
                    { IsBackground = true };
                if (i == 3)
                    thread = new Thread(() =>
                    {
                        Decode(
                            "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1",
                              w/2,h / 2 , w / 2 - borderSize, h/2- borderSize

                            );


                    })
                    { IsBackground = true };
                thread?.Start();
            }
        }

        public void Decode(string address, int x, int y, int w, int h)
        {
            ffmpeg.avformat_network_init();
            AVDictionary* options1 = null;

            ffmpeg.av_dict_set(&options1, "buffer_size", "100000", 0);
            ffmpeg.av_dict_set(&options1, "rtsp_transport", "tcp", 0);
            ffmpeg.av_dict_set(&options1, "stimeout", "5000000", 0);
            ffmpeg.av_dict_set(&options1, "max_delay", "500000", 0);

            //1:打开输入流文件

            AVFormatContext* fctx = ffmpeg.avformat_alloc_context();


            if (ffmpeg.avformat_open_input(&fctx, address, null, &options1) < 0)
            {

            }
            if (ffmpeg.avformat_find_stream_info(fctx, null) < 0)
            {

                ffmpeg.avformat_close_input(&fctx);

                ffmpeg.avformat_free_context(fctx);


            }


            AVCodecContext* codec_ctx = null;
            AVCodec* codec = null;
            AVPacket packet;
            AVFrame* frame = ffmpeg.av_frame_alloc();
            int ret;
            codec_ctx = ffmpeg.avcodec_alloc_context3(null);
            ffmpeg.avcodec_parameters_to_context(codec_ctx, fctx->streams[0]->codecpar);

            codec = ffmpeg.avcodec_find_decoder(codec_ctx->codec_id);
            if (codec == null)
            {

            }
            // 打开解码器
            if (ffmpeg.avcodec_open2(codec_ctx, codec, null) < 0)
            {

            }
         

            while (ffmpeg.av_read_frame(fctx, &packet) >= 0)
            {
                if (packet.stream_index == 0)
                {
                    ret = ffmpeg.avcodec_send_packet(codec_ctx, &packet);
                    if (ret < 0)
                    {
                        //error
                    }


                    ret = ffmpeg.avcodec_receive_frame(codec_ctx, frame);
                    if (ret < 0)
                    {
                        //error
                    }

                    AutoResetEvent signal = new AutoResetEvent(false);
                    //sdl播放
                    ThreadQueue.AddTask(() =>
                    {
                        nint texture = SDL.CreateTexture(render, SDL.PixelFormat.IYUV, TextureAccess.Streaming, 1920, 1080);
                        SDL.UpdateYUVTexture(texture, nint.Zero, (nint)frame->data[0], frame->linesize[0], (nint)frame->data[1], frame->linesize[1], (nint)frame->data[2], frame->linesize[2]);
                        FRect fRect = new FRect();
                        fRect.X = x;
                        fRect.Y = y;
                        fRect.W = w;
                        fRect.H = h;
                        SDL.RenderTexture(render, texture, nint.Zero, fRect);

                        SDL.SetRenderTarget(render, nint.Zero);
                        SDL.RenderTexture(render, targetTexture, nint.Zero, nint.Zero);

                        SDL.SetRenderTarget(render, targetTexture);
                        SDL.RenderPresent(render);
                        SDL.DestroyTexture(texture);
                        signal.Set();
                    });
                    signal.WaitOne();
                }
            }

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