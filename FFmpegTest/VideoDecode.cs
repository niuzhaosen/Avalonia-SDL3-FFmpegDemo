using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using FFmpeg.AutoGen.Abstractions;


namespace FFmpegTest;

public unsafe class VideoDecode
{


    /// <summary>
    /// 解码器ID
    /// </summary>
    AVCodecID videoCodecID;

    /// <summary>
    /// 媒体数据包
    /// </summary>
    AVPacket* avPacket;

    /// <summary>
    /// 媒体帧数据
    /// </summary>
    AVFrame* avFrame;

    /// <summary>
    /// 解码器
    /// </summary>
    AVCodec* avCodec;

    /// <summary>
    /// 编解码上下文
    /// </summary>
    AVCodecContext* avCodecContext;

    public void InitDecoder()
    {
        videoCodecID = AVCodecID.AV_CODEC_ID_H264;
        avPacket = ffmpeg.av_packet_alloc();
        if (avPacket == null)
        {
            Debug.Fail("AV_packet_alloc() returned null");
        }

        avFrame = ffmpeg.av_frame_alloc();
        if (avFrame == null)
        {
            Debug.Fail("AV_frame_alloc() returned null");
        }

        avCodec = ffmpeg.avcodec_find_decoder(videoCodecID);
        avCodecContext = ffmpeg.avcodec_alloc_context3(avCodec);
        if (avCodecContext != null)
        {
            //avCodecContext->width = 1280;
            //avCodecContext->height = 720;
            //avCodecContext->time_base.num = 1;
            //avCodecContext->time_base.den = 90000;
            //avCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_LOW_DELAY;
            if (ffmpeg.avcodec_open2(avCodecContext, avCodec, null) < 0)
            {
                Debug.Fail("AV_codec_open2() returned null");
            }
        }
    }

    public byte[] DecoderRTSP(byte* data, int count,IntPtr s)
    {

        avPacket->data = data;
        avPacket->pts = avPacket->dts = 0;
        avPacket->duration = 0;
        avPacket->size = count;
        avPacket->flags = 1;
        int resule = ffmpeg.avcodec_send_packet(avCodecContext, avPacket);
        if (resule != 0)
        {
            Console.WriteLine("填充失败");
        }

        resule = ffmpeg.avcodec_receive_frame(avCodecContext, avFrame);
        if (resule != 0)
        {
            Console.WriteLine("解码失败");
        }
      


        AVFrame* dst_frame = ffmpeg.av_frame_alloc();
        dst_frame->format = (int)AVPixelFormat.AV_PIX_FMT_BGRA;
        dst_frame->width = avFrame->width;
        dst_frame->height = avFrame->height;
       

        

        // 创建转换上下文
        SwsContext* ctx = ffmpeg.sws_getContext(
            avFrame->width,
            avFrame->height,
            (AVPixelFormat)avFrame->format,
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
        resule = ffmpeg.av_image_fill_arrays(ref TargetData, ref TargetLinesize, (byte*)FrameBufferPtr, (AVPixelFormat)dst_frame->format, dst_frame->width,
              dst_frame->height, 1);
        // 利用转换器将yuv 图像数据转换成指定的格式数据
        ffmpeg.sws_scale(ctx, avFrame->data, avFrame->linesize, 0, avFrame->height, TargetData, TargetLinesize);
        var data1 = new byte_ptr8();
        data1.UpdateFrom(TargetData);
        var linesize = new int8();
        linesize.UpdateFrom(TargetLinesize);
        //创建一个字节数据，将转换后的数据从内存中读取成字节数组
        byte[] bytes = new byte[1920 * 1080 * 4];
        Buffer.MemoryCopy((void*)data1[0], (void*)s, bytes.Length, bytes.Length);
        

        Marshal.FreeHGlobal(FrameBufferPtr);
        ffmpeg.sws_freeContext(ctx);

        return bytes;



    }
   
}