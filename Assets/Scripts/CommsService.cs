using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class CommsService : MonoBehaviour
{
    public enum Cmd
    {
        Debug,
        QuestConfirmation,
        FileTx,
        Heartbeat
    }

    const int REALIVATION_PORT = 33134;
    const int HEARTBEAT_INTERVAL_MS = 2000;
    private TcpClient _activePcClient;
    private NetworkStream _activeStream;
    private CancellationTokenSource _heartbeatCts;

    void Start()
    {
        
    }

    private static async Task SendQuestConfirmationAsync(NetworkStream stream)
    {
        Console.WriteLine("Sending Quest confirmation...");
        await stream.WriteAsync(new byte[] { (byte)Cmd.QuestConfirmation });
        byte[] payload = { 1, 0 };
        byte[] payloadLenBytes = BitConverter.GetBytes((UInt32)payload.Length);
        await stream.WriteAsync(payloadLenBytes);
        await stream.WriteAsync(payload);
        await stream.FlushAsync();
        Console.WriteLine("Confirmation sent successfully.");
    }

    private static async Task SendHeartbeatAsync(NetworkStream stream)
    {
        try
        {
            if (!stream.CanWrite)
            {
                throw new Exception("Can't write to the stream");
            }
            await stream.WriteAsync(new byte[] { (byte)Cmd.Heartbeat });
            byte[] payloadLenBytes = BitConverter.GetBytes((UInt32)0);
            await stream.WriteAsync(payloadLenBytes);
            await stream.FlushAsync();
            Console.WriteLine("(heartbeat)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Heartbeat failed... {ex.Message}");
        }
    }

    private async Task HeartbeatLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(HEARTBEAT_INTERVAL_MS, ct);
                await SendHeartbeatAsync(_activeStream);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Heartbeats stopped cuz it was cancelled");
        }
        catch (Exception e)
        {
            Debug.LogError($"Heartbeat exception: {e.Message}");
            DisposeConnection();
        }
    }

    public async Task<string> InitConnection(string pcIpAddress)
    {
        string err = "";
        if (_activePcClient != null && _activePcClient.Connected)
        {
            Debug.LogWarning($"Cutting existing connection.");
            DisposeConnection();
        }
        try
        {
            _activePcClient = new TcpClient();
            await _activePcClient.ConnectAsync(pcIpAddress, REALIVATION_PORT);
            Debug.Log($"Connected to {pcIpAddress}");
            _activeStream = _activePcClient.GetStream();
            await SendQuestConfirmationAsync(_activeStream);
            _heartbeatCts = new();
            _ = HeartbeatLoopAsync(_heartbeatCts.Token);
        }
        catch (Exception e)
        {
            Debug.LogError($"Couldn't init connection with {pcIpAddress}: {e.Message}");
            err = e.Message;
            DisposeConnection();
        }
        return err;
    }
    public void DisposeConnection()
    {
        if (_heartbeatCts != null)
        {
            _heartbeatCts.Cancel();
            _heartbeatCts.Dispose();
            _heartbeatCts = null;
        }
        _activeStream?.Dispose();
        _activeStream = null;
        _activePcClient?.Dispose();
        _activePcClient = null;
        Debug.Log("Connection disposed");
    }

    private void OnDestroy()
    {
        DisposeConnection();
    }

    public string IpStrToHostname(string ipAddr)
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse(ipAddr);
            IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
            return hostEntry.HostName;
        }
        catch (System.Net.Sockets.SocketException)
        {
            return "No name";
        }
        catch (FormatException)
        {
            return "Bad address";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
