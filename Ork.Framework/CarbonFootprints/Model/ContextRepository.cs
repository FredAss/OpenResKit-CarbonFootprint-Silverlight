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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using Ork.Framework.DomainModelService;
using Ork.Framework.PositionInformationService;

namespace Ork.Framework.CarbonFootprints.Model
{
  [Export]
  public class ContextRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private readonly SettingsProvider m_SettingsProvider;
    private DomainModelContext m_Context;
    private PositionInformationServiceClient m_PositionInformationServiceClient;

    [ImportingConstructor]
    public ContextRepository(Func<DomainModelContext> createMethod, SettingsProvider settingsProvider)
    {
      m_CreateMethod = createMethod;
      m_SettingsProvider = settingsProvider;
      m_SettingsProvider.ConnectionChanged += (sender, args) => Initialize();
      Initialize();
    }

    public DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; private set; }

    public DataServiceCollection<CarbonFootprint> CarbonFootprints { get; private set; }

    private void LoadResponsibleSubjects()
    {
      ResponsibleSubjects = new DataServiceCollection<ResponsibleSubject>(m_Context);
      var query = m_Context.ResponsibleSubjects.Expand("OpenResKit.DomainModel.Employee/Groups");
      ResponsibleSubjects.LoadCompleted += (s, e) => LoadCarbonFootprints();
      ResponsibleSubjects.LoadAsync(query);
    }

    private void LoadCarbonFootprints()
    {
      CarbonFootprints = new DataServiceCollection<CarbonFootprint>(m_Context);
      var query = m_Context.CarbonFootprints.Expand("Positions")
                           .Expand("Positions/OpenResKit.DomainModel.CarbonFootprintPosition/ResponsibleSubject")
                           .Expand("Positions/OpenResKit.DomainModel.AirportBasedFlight/Airports");
      CarbonFootprints.LoadCompleted += (s, e) => RaiseEvent(ContextChanged);
      CarbonFootprints.LoadAsync(query);
    }

    private void PositionInformationServiceClientOnCalculateCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
    {
      Initialize();
    }

    public void Calculate(int carbonFootprintId)
    {
      m_PositionInformationServiceClient.CalculateAsync(carbonFootprintId);
    }

    private void Initialize()
    {
      m_PositionInformationServiceClient = new PositionInformationServiceClient("BasicHttpBinding_PositionInformationService",
        string.Format("http://{0}:{1}/PositionInformationService", m_SettingsProvider.Url, m_SettingsProvider.Port));
      m_PositionInformationServiceClient.ClientCredentials.UserName.UserName = m_SettingsProvider.UserName;
      m_PositionInformationServiceClient.ClientCredentials.UserName.Password = m_SettingsProvider.Password;
      m_PositionInformationServiceClient.CalculateCompleted += PositionInformationServiceClientOnCalculateCompleted;
      m_Context = m_CreateMethod();
      LoadResponsibleSubjects();
    }

    public void Save()
    {
      if (m_Context.ApplyingChanges)
      {
        return;
      }

      var result = m_Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
                                                                        {
                                                                          var dm = (DomainModelContext) r.AsyncState;
                                                                          dm.EndSaveChanges(r);
                                                                          RaiseEvent(SaveCompleted);
                                                                        }, m_Context);
    }

    private void RaiseEvent(EventHandler eventHandler)
    {
      if (eventHandler != null)
      {
        eventHandler(this, new EventArgs());
      }
    }

    public event EventHandler ContextChanged;
    public event EventHandler SaveCompleted;
  }
}