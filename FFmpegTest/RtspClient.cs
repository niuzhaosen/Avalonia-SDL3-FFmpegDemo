

using FFmpeg.AutoGen.Abstractions;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFmpegTest;

public class RtspClientTest
{
    showVideo show;
    IntPtr ptr;

    public RtspClientTest(showVideo showVideo, IntPtr x, string rtspAddress)
    {
        show = showVideo; ptr = x;
        RtspAddress = rtspAddress;

    }
    private CancellationTokenSource cancellationTokenSource;

    string RtspAddress;


    private VideoDecode videoDecode;
    public unsafe void PlayByFFmpeg()
    {
        ffmpeg.avformat_network_init();
        AVDictionary* options1 = null;

        ffmpeg.av_dict_set(&options1, "buffer_size", "100000", 0);
        ffmpeg.av_dict_set(&options1, "rtsp_transport", "tcp", 0);
        ffmpeg.av_dict_set(&options1, "stimeout", "5000000", 0);
        ffmpeg.av_dict_set(&options1, "max_delay", "500000", 0);

        //1:打开输入流文件

        AVFormatContext* fctx1 = ffmpeg.avformat_alloc_context();
  

        if (ffmpeg.avformat_open_input(&fctx1, RtspAddress, null, &options1) < 0)
        {

        }
        if (ffmpeg.avformat_find_stream_info(fctx1, null) < 0)
        {

            ffmpeg.avformat_close_input(&fctx1);

            ffmpeg.avformat_free_context(fctx1);


        }
        ////寻找对应的视频流索引
        //for (int i = 0; i < fctx1->nb_streams; i++)
        //{
        //    if (fctx1->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
        //    {

        //    }
        //}

        AVCodecContext* codec_ctx = null;
        AVCodec* codec = null;
        AVPacket packet;
        AVFrame* frame = ffmpeg.av_frame_alloc();
        int ret;


        // 获取解码器上下文
        codec_ctx = ffmpeg.avcodec_alloc_context3(null);
        ffmpeg.avcodec_parameters_to_context(codec_ctx, fctx1->streams[0]->codecpar);

        // 寻找解码器
        codec = ffmpeg.avcodec_find_decoder(codec_ctx->codec_id);
        if (codec == null)
        {

        }

        // 打开解码器
        if (ffmpeg.avcodec_open2(codec_ctx, codec, null) < 0)
        {

        }

        while (ffmpeg.av_read_frame(fctx1, &packet) >= 0)
        {
            if (packet.stream_index == 0)
            {
                ret = ffmpeg.avcodec_send_packet(codec_ctx, &packet);
                if (ret < 0)
                {

                }


                ret = ffmpeg.avcodec_receive_frame(codec_ctx, frame);
                if (ret < 0)
                {

                }

                ffmpeg.av_packet_unref(&packet);
                AVFrame* dst_frame = ffmpeg.av_frame_alloc();
                dst_frame->format = (int)AVPixelFormat.AV_PIX_FMT_BGRA;
                dst_frame->width = frame->width;
                dst_frame->height = frame->height;




                // 创建转换上下文
                SwsContext* ctx = ffmpeg.sws_getContext(
                    frame->width,
                frame->height,
                    (AVPixelFormat)frame->format,
                     dst_frame->width,
                      dst_frame->height,
                    (AVPixelFormat)dst_frame->format,
                    ffmpeg.SWS_BICUBIC,
                    null, null, null);
                //获取转换后图像的 缓冲区大小
                var bufferSize = ffmpeg.av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_BGRA, dst_frame->width,
                      dst_frame->height, 1);
                //获取转换后图像的 缓冲区大小

                //创建一个指针
                var FrameBufferPtr = Marshal.AllocHGlobal(bufferSize);
                var TargetData = new byte_ptr4();
                var TargetLinesize = new int4();
                ffmpeg.av_image_fill_arrays(ref TargetData, ref TargetLinesize, (byte*)FrameBufferPtr, (AVPixelFormat)dst_frame->format, dst_frame->width,
                    dst_frame->height, 1);
                // 利用转换器将yuv 图像数据转换成指定的格式数据
                ffmpeg.sws_scale(ctx, frame->data, frame->linesize, 0, frame->height, TargetData, TargetLinesize);



                var data1 = new byte_ptr8();
                data1.UpdateFrom(TargetData);
                var linesize = new int8();
                linesize.UpdateFrom(TargetLinesize);
                //创建一个字节数据，将转换后的数据从内存中读取成字节数组
                byte[] bytes = new byte[1920 * 1080 * 4];
                Buffer.MemoryCopy((void*)data1[0], (void*)ptr, bytes.Length, bytes.Length);



                show();
                Marshal.FreeHGlobal(FrameBufferPtr);
                ffmpeg.sws_freeContext(ctx);

            }
        }
    }


    public void PlayByRtspClient()
    {
        videoDecode = new VideoDecode();
        videoDecode.InitDecoder();
        try
        {
            var serverUri = new Uri(RtspAddress);
            var connectionParameters = new ConnectionParameters(serverUri);
            cancellationTokenSource = new CancellationTokenSource();
            Task connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);
        }
        catch (Exception)
        {
            Console.WriteLine("Rtsp取流失败!");
        }
    }

    private async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
    {
        try
        {
            TimeSpan delay = TimeSpan.FromSeconds(5);

            using (var rtspClient = new RtspClientSharp.RtspClient(connectionParameters))
            {
                rtspClient.FrameReceived += RtspClient_FrameReceived;


                while (true)
                {
                    Console.WriteLine("Connecting...");

                    try
                    {
                        await rtspClient.ConnectAsync(token);

                    }
                    catch (OperationCanceledException)
                    {
                        rtspClient.FrameReceived -= RtspClient_FrameReceived;
                        return;
                    }
                    catch (RtspClientException e)
                    {
                        Console.WriteLine(e.ToString());
                        await Task.Delay(delay, token);
                        continue;
                    }

                    Console.WriteLine("Connected.");

                    try
                    {
                        await rtspClient.ReceiveAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (RtspClientException e)
                    {
                        Console.WriteLine(e.ToString());
                        await Task.Delay(delay, token);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
    public delegate void showVideo();
    //流接受并进行解码
    private void RtspClient_FrameReceived(object sender, RtspClientSharp.RawFrames.RawFrame e)
    {

        if (e is RawH264Frame)
        {


            unsafe
            {
                bool isgj = false;
                byte[] array;
                if (e is RtspClientSharp.RawFrames.Video.RawH264IFrame frame)
                {
                    IntPtr memory = Marshal.AllocHGlobal(frame.FrameSegment.Count + frame.SpsPpsSegment.Count);
                    array = frame.SpsPpsSegment.Array.Concat(frame.FrameSegment).ToArray();
                    isgj = true;
                }
                else
                {
                    array = e.FrameSegment.Array;
                }

                //if(e.sa)


                // 固定内存区域以获取指针
                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    byte* data = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0).ToPointer();
                 
                   videoDecode.DecoderRTSP(data, array.Length, ptr, isgj);
                 
                  
                    show();
                }
                finally
                {
                    // 释放GCHandle
                    handle.Free();
                }

            }


        }

        if (e is RawH265Frame)
        {


            unsafe
            {
                bool isgj = false;
                byte[] array;
                if (e is RtspClientSharp.RawFrames.Video.RawH265IFrame frame)
                {
                    IntPtr memory = Marshal.AllocHGlobal(frame.FrameSegment.Count + frame.ParametersBytesSegment.Count);
                    array = frame.ParametersBytesSegment.Array.Concat(frame.FrameSegment).ToArray();
                    isgj = true;
                }
                else
                {
                    array = e.FrameSegment.Array;
                }

                //if(e.sa)


                // 固定内存区域以获取指针
                GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    byte* data = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(array, 0).ToPointer();
                  
                   videoDecode.DecoderRTSP(data, array.Length, ptr, isgj);
                 
                    show();
                }
                finally
                {
                    // 释放GCHandle
                    handle.Free();
                }

            }


        }
    }

    //停止取流
    public async void stopPlay()
    {
        cancellationTokenSource.Cancel();
    }
}