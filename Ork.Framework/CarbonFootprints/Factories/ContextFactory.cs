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
using System.ComponentModel.Composition;
using System.Net;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.DomainModelService;

namespace Ork.Framework.CarbonFootprints.Factories
{
  public class ContextFactory
  {
    private readonly SettingsProvider m_SettingsProvider;

    [ImportingConstructor]
    public ContextFactory(SettingsProvider settingsProvider)
    {
      m_SettingsProvider = settingsProvider;
    }

    [Export]
    public DomainModelContext CreateContext()
    {
      var uri = new Uri(string.Format("http://{0}:{1}/OpenResKitHub", m_SettingsProvider.Url, m_SettingsProvider.Port));
      var dms = new DomainModelContext(uri);
      dms.Credentials = new NetworkCredential(m_SettingsProvider.UserName, m_SettingsProvider.Password);
      return dms;
    }
  }
}