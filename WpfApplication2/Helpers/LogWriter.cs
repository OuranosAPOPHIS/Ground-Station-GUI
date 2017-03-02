using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APOPHIS.GroundStation.Helpers
{
  class LogWriter
  {
    //
    // This logger will write the file to the desktop.
    private string m_exePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private string FileName = string.Empty;
    private bool FirstTime = true;

    public void LogCreator(string fileName)
    {
      FileName = fileName;

      if (!File.Exists(m_exePath + "\\" + FileName + ".csv")) {
        File.Create(m_exePath + "\\" + FileName + ".csv");
        FileName = FileName + ".csv";
      }
      else
      {
        do
        {
          FileName += 1;
        } while (File.Exists(m_exePath + "\\" + FileName + ".csv"));

        FileName += ".csv";
        File.Create(m_exePath + "\\" + FileName);
      }

      //
      // Initialize the log file.
      LogWrite(null);
    }

    public void LogWrite(string logMessage)
    {
      try
      {
        using (StreamWriter w = File.AppendText(m_exePath + "\\" + FileName))
        {
          try
          {
            if (FirstTime)
            {
              w.WriteLine("Ground Station GUI Log Data\r\n{0} {1}:", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
              FirstTime = false;
            }
            else
            {
              w.WriteLine(logMessage);
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      catch (Exception ex)
      {
      }
    }
  }
}
