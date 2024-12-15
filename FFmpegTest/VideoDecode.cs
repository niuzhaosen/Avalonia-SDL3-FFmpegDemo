using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen.Abstractions;
using RtspClientSharp;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;
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
            avCodecContext->width = 1920;
            avCodecContext->height = 1080;
            avCodecContext->time_base.num = 1;
            avCodecContext->time_base.den = 9000;
            avCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_LOW_DELAY;
            if (ffmpeg.avcodec_open2(avCodecContext, avCodec, null) < 0)
            {
                Debug.Fail("AV_codec_open2() returned null");
            }
        }
    }

    public void DecoderRTSP(byte[] ss)
    {
        fixed (byte* ptr = ss)
        {
            avPacket->data = ptr;
            int resule = ffmpeg.avcodec_send_packet(avCodecContext, avPacket);
            if (resule!=0)
            {
                
            }

            resule = ffmpeg.avcodec_receive_frame(avCodecContext, avFrame);
            if (resule !=0)
            {
                
            }
        }
       
    }
}