using System;
using System.ComponentModel.Composition;
using System.IO.IsolatedStorage;

namespace Ork.Framework.CarbonFootprints.Model
{
  [Export]
  public class SettingsProvider
  {
    private readonly IsolatedStorageSettings m_AppSettings;

    public SettingsProvider()
    {
      m_AppSettings = IsolatedStorageSettings.ApplicationSettings;
      InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
      if (!m_AppSettings.Contains("port"))
      {
        m_AppSettings.Add("url", "localhost");
        m_AppSettings.Add("port", "7000");
        m_AppSettings.Add("userName", "root");
        m_AppSettings.Add("password", "ork123");
      }
    }

    public event EventHandler ConnectionChanged;

    public string Url
    {
      get
      {
        return (string) m_AppSettings["url"];
      }
      private set
      {
        m_AppSettings["url"] = value;
      }
    }

    public string Port
    {
      get
      {
        return (string) m_AppSettings["port"];
      }
      private set
      {
        m_AppSettings["port"] = value;
      }
    }
    public string UserName
    {
      get
      {
        return (string) m_AppSettings["userName"];
      }
      private set
      {
        m_AppSettings["userName"] = value;
      }
    }
    public string Password
    {
      get
      {
        return (string) m_AppSettings["password"];
      }
      private set
      {
        m_AppSettings["password"] = value;
      }
    }

    public void ResetConnectionSettings(string url, string port, string userName, string password)
    {
      Url = url;
      Port = port;
      UserName = userName;
      Password = password;
      m_AppSettings.Save();
      if (ConnectionChanged != null)
      {
        ConnectionChanged(this, new EventArgs());
      }
    }
  }
}
