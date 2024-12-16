using FFmpeg.AutoGen.Bindings.DynamicallyLoaded;
namespace FFmpegTest;

public  class FFmpegInit
{
    public static void Init()
    {

        if( Environment.OSVersion.Platform== PlatformID.Win32NT)
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
            DynamicallyLoadedBindings.LibrariesPath = AppDomain.CurrentDomain.BaseDirectory + "linux_x64";
        }
        
        
        DynamicallyLoadedBindings.Initialize();


      
    }
}