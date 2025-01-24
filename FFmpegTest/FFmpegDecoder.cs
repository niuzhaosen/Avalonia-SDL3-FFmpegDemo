

using FFmpeg.AutoGen.Abstractions;

using Silk.NET.SDL;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;





namespace FFmpegTest;

public unsafe class FFmpegDecoder
{


    Sdl sdl;
    IntPtr ptr;
    nint m_pTexture;
    Renderer* render;
 

    string rtspAddress;
    public delegate void showVideo();
    showVideo show;
    public FFmpegDecoder(string rtspAddress, nint m_pTexture, Renderer* render, Sdl sdl, showVideo showVideo, nint ptr)
    {
        this.sdl = sdl;
        this.render = render;
        this.m_pTexture = m_pTexture;
        this.rtspAddress = rtspAddress;
      
        this.show = showVideo;
        this.ptr = ptr;

    }





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


        if (ffmpeg.avformat_open_input(&fctx1, rtspAddress, null, &options1) < 0)
        {

        }
        if (ffmpeg.avformat_find_stream_info(fctx1, null) < 0)
        {

            ffmpeg.avformat_close_input(&fctx1);

            ffmpeg.avformat_free_context(fctx1);


        }

        int index = 0;
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


        Texture* texture = sdl.CreateTexture(render, (uint)PixelFormatEnum.Iyuv, (int)TextureAccess.Streaming, 1920, 1080);

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


                //sdl播放

                sdl.UpdateYUVTexture(texture, null, frame->data[0], frame->linesize[0], frame->data[1], frame->linesize[1], frame->data[2], frame->linesize[2]);

                sdl.RenderCopy(render, texture, null, null);

                sdl.RenderPresent(render);
                //原生图片刷新

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
                //byte[] bytes = new byte[1920 * 1080 * 4];
                //IntPtr bytesPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
                Buffer.MemoryCopy((void*)data1[0], (void*)ptr, frame->width * frame->height*4, frame->width * frame->height * 4);


                Marshal.FreeHGlobal(FrameBufferPtr);
                ffmpeg.sws_freeContext(ctx);
                show();
            }
        }
    }



}