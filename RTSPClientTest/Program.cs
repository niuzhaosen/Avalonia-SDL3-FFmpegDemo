// See https://aka.ms/new-console-template for more information
using RtspClientSharp;
using RtspClientSharp.Rtsp;
var serverUri = new Uri("rtsp://admin:asdqwe123@192.168.1.66:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1");

var connectionParameters = new ConnectionParameters(serverUri);
var cancellationTokenSource = new CancellationTokenSource();
connectionParameters.RtpTransport = RtpTransportProtocol.TCP;
TimeSpan delay = TimeSpan.FromSeconds(5);
using(var rtspClient = new RtspClient(connectionParameters))
{
	
	rtspClient.FrameReceived +=
		(sender, frame) => Console.WriteLine($"New frame {frame.Timestamp}: {frame.GetType().Name}");
	
	while (true)
	{
		Console.WriteLine("Connecting...");

		try
		{
			await rtspClient.ConnectAsync(cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (RtspClientException e)
		{
			Console.WriteLine(e.ToString());
			await Task.Delay(delay, cancellationTokenSource.Token);
			continue;
		}

		Console.WriteLine("Connected.");

		try
		{
			await rtspClient.ReceiveAsync(cancellationTokenSource.Token);
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (RtspClientException e)
		{
			Console.WriteLine(e.ToString());
			await Task.Delay(delay, cancellationTokenSource.Token);
		}
	}
}
Console.ReadLine();