
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FFmpegTest;

public class RtspClientTest
{
    showVideo show;
    IntPtr ptr;
    public RtspClientTest(showVideo showVideo,IntPtr x)
    {
         show = showVideo;ptr = x;
    }
    private CancellationTokenSource cancellationTokenSource;

    string RtspLive =
        "rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1";

    private VideoDecode videoDecode;
    public void Run()
    {
        videoDecode = new VideoDecode();
        videoDecode.InitDecoder();
        try
        {
            var serverUri = new Uri(RtspLive);
            var connectionParameters = new ConnectionParameters(serverUri);
            cancellationTokenSource = new CancellationTokenSource();
            Task connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);
        }
        catch (Exception)
        {
            Console.WriteLine("Rtsp取流失败!");
        }
    }
    
    //开始进行异步连接取流
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
    public delegate void showVideo(byte[] data);
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
                    Stopwatch stopwatch =new Stopwatch();
                    stopwatch.Start();
                   // IntPtr sss = Marshal.AllocHGlobal(1920 * 1080 * 3);
                    byte[] outdata = videoDecode.DecoderRTSP(data, array.Length, ptr, isgj);
                    stopwatch.Stop();
                    Console.WriteLine("解码+转码+一次内存拷贝时间："+ stopwatch.Elapsed.TotalMilliseconds);
                  //  Marshal.FreeHGlobal(sss);
                    show(outdata);
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
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    // IntPtr sss = Marshal.AllocHGlobal(1920 * 1080 * 3);
                    byte[] outdata = videoDecode.DecoderRTSP(data, array.Length, ptr, isgj);
                    stopwatch.Stop();
                    Console.WriteLine("解码+转码+一次内存拷贝时间：" + stopwatch.Elapsed.TotalMilliseconds);
                    //  Marshal.FreeHGlobal(sss);
                    show(outdata);
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