// See https://aka.ms/new-console-template for more information

using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;
using FFmpeg.AutoGen.Abstractions;
using FFmpegTest;


unsafe
{
    Console.WriteLine("Hello, World!");
    FFmpegInit.Init();
    Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");

    RtspClientTest rtspClient = new RtspClientTest();
    rtspClient.Run();
    Console.ReadLine();
}