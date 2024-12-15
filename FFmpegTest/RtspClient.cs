
using RtspClientSharp;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;

namespace FFmpegTest;

public class RtspClientTest
{
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

    //流接受并进行解码
    private void RtspClient_FrameReceived(object sender, RtspClientSharp.RawFrames.RawFrame e)
    {
        if (e is RawH264Frame)
        {
            videoDecode.DecoderRTSP(e.FrameSegment.ToArray());
           
        }
    }

    //停止取流
    public async void stopPlay()
    {
        cancellationTokenSource.Cancel();
    }
}