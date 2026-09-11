using Foster.Audio;
using System.ComponentModel.Design;
using FAudio = Foster.Audio.Audio;

namespace ITalent.Games.FosterAudio;

internal class Program
{
    static void Main(string[] args)
    {
        FAudio.Startup();

        Console.WriteLine("L = Load");
        bool isRunning = true;
        ManagedSound? crash = null, snare = null, kick = null, openhat = null, closedhat = null;
        while (isRunning)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.L)
            {
                snare = new("Resources\\snare-trimmed.wav");
                kick = new("Resources\\kick-trimmed.wav");
                crash = new("Resources\\crash-trimmed.wav");
                openhat = new("Resources\\open-hat-trimmed.wav");
                closedhat = new("Resources\\closed-hat-trimmed.wav");
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("D = Kick | A = Snare | G = Crash | H = Openhat | J = Closedhat");
            }
            else if (key.Key == ConsoleKey.A)
            {
                snare?.OneShot();
            }
            else if (key.Key == ConsoleKey.D)
            {
                kick?.OneShot();
            }
            else if (key.Key == ConsoleKey.G)
            {
                crash?.OneShot();
            }
            else if (key.Key == ConsoleKey.H)
            {
                openhat?.OneShot();
            }
            else if (key.Key == ConsoleKey.J)
            {
                openhat?.Stop();
                closedhat?.OneShot();
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                isRunning = false;
            }
        }

        FAudio.Shutdown();
    }
}

class ManagedSound : IDisposable
{
    private Sound asset;
    private SoundInstance instance;

    public ManagedSound(string path)
    {
        var encodedData = File.ReadAllBytes(path);
        var format = AudioFormat.F32;
        var channels = 0;
        var sampleRate = FAudio.SampleRate;
        byte[]? data;

        if (Sound.TryDecode(encodedData, ref format, ref channels, ref sampleRate, out var frameCount, out data))
        {
            asset = new Sound(data!, format, channels, sampleRate, frameCount);
            instance = asset.CreateInstance();
            instance.Protected = true;
            instance.Volume = 1.0f;
        }
        else
        {
            throw new Exception("Couldn't parse encoded data");
        }
    }

    public void OneShot()
    {
        if (instance.Playing) instance.Stop();
        instance.Play();
    }

    public void Stop()
    {
        if (instance.Playing) instance.Stop();
    }

    public void Dispose()
    {
        asset.Dispose();
    }
}