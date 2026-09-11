using System.IO.Ports;

namespace ITalent.Games.Serial;

/*
 * 
 * 1. Installeer V210 bridge drivers voor USB --> Serial
 * 
 *    https://www.silabs.com/software-and-tools/usb-to-uart-bridge-vcp-drivers?tab=downloads
 *    
 * 2. Vul de juiste COM port en baud rate in
 * 
 * 
 * 
 * Wellicht werkt het alleen via bluetooth:
 
 *    https://www.techcoil.com/blog/how-to-write-a-c-program-to-communicate-with-an-esp32-development-board-via-bluetooth-serial/
 * 
 */

internal class Program
{
  static async Task Main(string[] args)
  {
    await new App().Run();
  }
}

class App
{
  private SerialPort serialPort;

  public async Task Run()
  {
    serialPort = new("COM3", 9600); // 921600
    try
    {
      serialPort.Open();
    }
    catch
    {
      Console.WriteLine("Could not open serial port.");
      return;
    }


    //serialPort.WriteLine("SET TOUCH_READ T5");
    while (true)
    {
      string s = serialPort.ReadLine();
      if (!string.IsNullOrEmpty(s))
      {
        Console.WriteLine(s);
      }
      await Task.Delay(100);
    }
  }
}