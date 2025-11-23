using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ConcurrentQueue<string> _receivedFileQueue = new();


    void Update()
    {
        if (!_receivedFileQueue.IsEmpty && _receivedFileQueue.TryDequeue(out string filePath))
        {
            Debug.Log($"Processing file {Path.GetFileName(filePath)}");
            ProcessRxFile(filePath);
        }
    }

    private void ProcessRxFile(string path)
    {
        string[] supportedTypes = { "obj", "glb", "gltf", "fbx", "stl", "ply", "3mf", "dae" };
        string extension = Path.GetExtension(path);
        string fileType = extension.TrimStart('.').ToLowerInvariant();
        if (!supportedTypes.Contains(fileType))
        {
            Debug.LogError($"Unsupported file type {fileType}");
            return;
        }
        Transform head = Camera.main.transform;
        Vector3 pos = head.position + head.forward * 0.35f;
        Services.Get<ModelLoadingService>().ImportModelAsync(path, pos).ConfigureAwait(false);
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

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        Debug.Log("Receive loop started.");
        byte[] cmdBuffer = new byte[1];

        try
        {
            while (!ct.IsCancellationRequested && _activeStream != null && _activeStream.CanRead)
            {
                int bytesRead = await _activeStream.ReadAsync(cmdBuffer, 0, 1, ct);

                if (bytesRead == 0)
                {
                    Debug.LogWarning("Server closed the connection.");
                    DisposeConnection();
                    break;
                }
                Cmd cmd = (Cmd)cmdBuffer[0];
                switch (cmd)
                {
                    case Cmd.FileTx:
                        await HandleIncomingFileAsync(_activeStream, ct);
                        break;
                    default:
                        Debug.LogWarning($"Received unknown command byte: {cmd}");
                        break;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!ct.IsCancellationRequested)
            {
                Debug.LogError($"Receive Loop Error: {ex.Message}");
                // (but dont disconnect its ok)
            }
        }
    }

    private async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken ct)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer, offset, count - offset, ct);
            if (read == 0) throw new EndOfStreamException("Connection closed unexpectedly.");
            offset += read;
        }
    }


    private async Task HandleIncomingFileAsync(NetworkStream stream, CancellationToken ct)
    {
        Debug.Log("Starting File Reception...");

        try
        {
            byte[] buffer4 = new byte[4];
            await ReadExactlyAsync(stream, buffer4, 4, ct);
            await ReadExactlyAsync(stream, buffer4, 4, ct);
            uint fileNameLen = BitConverter.ToUInt32(buffer4, 0);
            byte[] nameBuffer = new byte[fileNameLen];
            await ReadExactlyAsync(stream, nameBuffer, (int)fileNameLen, ct);
            string fileName = Encoding.UTF8.GetString(nameBuffer);
            await ReadExactlyAsync(stream, buffer4, 4, ct);
            long contentLen = BitConverter.ToUInt32(buffer4, 0);
            Debug.Log($"Receiving file: '{fileName}' Size: {contentLen} bytes");
            string savePath = Path.Combine(Application.persistentDataPath, Services.TransferDirName, fileName);
            string dir = Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(savePath))
            {
                Debug.Log("Overwriting existing file");
                try
                {
                    File.SetAttributes(savePath, FileAttributes.Normal);
                    File.Delete(savePath);
                }
                catch (IOException ex)
                {
                    Debug.LogError($"File is locked, cant be deleted: {ex.Message}");
                    throw;
                }
            }
            using (FileStream fs = new(savePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[8192];
                long totalRead = 0;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                long lastLogTime = 0;
                while (totalRead < contentLen)
                {
                    int toRead = (int)Math.Min(buffer.Length, contentLen - totalRead);
                    int read = await stream.ReadAsync(buffer, 0, toRead, ct);
                    if (read == 0) throw new EndOfStreamException("Stream closed during file download");

                    await fs.WriteAsync(buffer, 0, read, ct);
                    totalRead += read;
                    if (sw.ElapsedMilliseconds - lastLogTime > 500)
                    {
                        lastLogTime = sw.ElapsedMilliseconds;
                        double percent = (double)totalRead / contentLen * 100.0;
                        double currentMb = totalRead / 1048576.0;
                        double totalMb = contentLen / 1048576.0;

                        Debug.Log($"Downloading {fileName}: {percent:F1}% ({currentMb:F2} MB / {totalMb:F2} MB)");
                    }
                }
            }

            Debug.Log($"<color=green>File saved to: {savePath}</color>");
            _receivedFileQueue.Enqueue(savePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error receiving file: {ex.Message}");
            throw;
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
            var connectTask = _activePcClient.ConnectAsync(pcIpAddress, REALIVATION_PORT);
            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
            {
                throw new Exception("Connection timed out");
            }
            await connectTask;
            Debug.Log($"Connected to {pcIpAddress}");
            _activeStream = _activePcClient.GetStream();
            await SendQuestConfirmationAsync(_activeStream);
            _heartbeatCts = new();
            _ = HeartbeatLoopAsync(_heartbeatCts.Token);
            _ = ReceiveLoopAsync(_heartbeatCts.Token);
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
