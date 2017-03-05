using System;
using System.IO;

namespace APOPHIS.GroundStation.Packet {
  class PacketLogger {
    //
    // This logger will write the file to the desktop.
    private string m_exePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private string FileName = string.Empty;
    private bool FirstTime = true;

    public void LogCreator(string fileName) {
      FileName = fileName;
            int num = 1;

      if (!File.Exists(m_exePath + "\\" + FileName + ".csv")) {
        File.Create(m_exePath + "\\" + FileName + ".csv").Close();
        FileName = FileName + ".csv";
      } else {
        do {
                num++;
            } while (File.Exists(m_exePath + "\\" + FileName + num + ".csv"));

        FileName += num + ".csv";
        File.Create(m_exePath + "\\" + FileName).Close();
      }

      //
      // Initialize the log file.
      LogWrite(null);
    }

    public void LogWrite(string logMessage) {
      try {
        using (StreamWriter w = File.AppendText(m_exePath + "\\" + FileName)) {
          try {
            if (FirstTime) {
              w.WriteLine($"Ground Station GUI Log Data\r\n{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}:");
              FirstTime = false;
            } else {
              w.WriteLine(logMessage);
            }
          } catch (Exception) { }
        }
      } catch (Exception) { }
    }
  }
}
