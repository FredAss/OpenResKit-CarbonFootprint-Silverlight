#region License

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0.html
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  
// Copyright (c) 2013, HTW Berlin

#endregion

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Net;
using Caliburn.Micro;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.Framework;
using Ork.Framework.Shell;

namespace Ork.Framework.Settings
{
  [Export(typeof (IWorkspace))]
  public class SettingsViewModel : Screen, IWorkspace
  {
    private readonly ContextRepository m_ContextRepository;
    private readonly ExampleDataProvider m_ExampleDataProvider;
    private readonly SettingsProvider m_SettingsProvider;
    private readonly TagColorProvider m_TagColorProvider;

    [ImportingConstructor]
    public SettingsViewModel(ContextRepository contextRepository, TagColorProvider tagColorProvider, ExampleDataProvider exampleDataProvider, SettingsProvider settingsProvider)
    {
      DisplayName = "Einstellungen";
      m_ContextRepository = contextRepository;
      m_TagColorProvider = tagColorProvider;
      m_ExampleDataProvider = exampleDataProvider;
      m_SettingsProvider = settingsProvider;
      m_ExampleDataProvider.SeedCompleted += (s, e) => Save();

      ServerUrl = m_SettingsProvider.Url;
      ServerPort = m_SettingsProvider.Port;
      UserName = m_SettingsProvider.UserName;
      Password = m_SettingsProvider.Password;
    }

    public string UserName { get; set; }
    public string Password { get; set; }
    public string ServerPort { get; set; }
    public string ServerUrl { get; set; }

    public ObservableCollection<TagColor> Tags
    {
      get { return m_TagColorProvider.TagColors; }
    }

    public string Status
    {
      get { return string.Empty; }
    }

    public void Show()
    {
      ((IConductor) Parent).ActivateItem(this);
    }

    public void CreateExampleData()
    {
      m_ExampleDataProvider.Seed();
    }

    public void Save()
    {
      m_TagColorProvider.SaveColorsToXml();
      m_SettingsProvider.ResetConnectionSettings(ServerUrl, ServerPort, UserName, Password);
    }

    public void TestConnection()
    {
      var uri = new Uri("http://" + ServerUrl + ":" + ServerPort + "/");
      var request = WebRequest.Create(uri);
      request.BeginGetResponse(RequestCallBack, request);
    }

    private void RequestCallBack(IAsyncResult result)
    {
      var req = (HttpWebRequest) result.AsyncState;

      try
      {
        req.EndGetResponse(result);
        ((ShellViewModel) Parent).Dialogs.ShowMessageBox("Die Verbindung wurde erfolgreich hergestellt!", "Information");
      }
      catch (WebException ex)
      {
        ((ShellViewModel) Parent).Dialogs.ShowMessageBox("Die Verbindung konnte nicht hergestellt werden.", "Warnung");
      }
    }
  }
}