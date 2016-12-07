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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Ork.Framework.CarbonFootprints.Interfaces;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.CarbonFootprints.ViewModels;
using Ork.Framework.DomainModelService;
using Ork.Framework.Settings;

namespace Ork.Framework.CarbonFootprints.Factories
{
  [Export]
  public class CarbonFootprintViewModelFactory
  {
    private readonly ContextRepository m_ContextRepository;
    private readonly IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> m_Factories;
    private readonly TagColorProvider m_TagColorProvider;

    [ImportingConstructor]
    public CarbonFootprintViewModelFactory([ImportMany] IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> factories, [Import] ContextRepository contextRepository,
      [Import] TagColorProvider tagColorProvider)
    {
      m_Factories = factories;
      m_ContextRepository = contextRepository;
      m_TagColorProvider = tagColorProvider;
    }

    public CarbonFootprintViewModel CreateFromExisting(CarbonFootprint model, string[] uniqueCarbonFootprintNames, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new CarbonFootprintViewModel(model, m_Factories, m_ContextRepository, uniqueCarbonFootprintNames, m_TagColorProvider, responsibleSubjects);
    }

    public CarbonFootprintViewModel CreateNew(string name, string description, int employees, string siteLocation, string[] uniqueCarbonFootprintNames, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(new CarbonFootprint
                                {
                                  Name = name,
                                  Description = description,
                                  Employees = employees,
                                  SiteLocation = siteLocation
                                }, uniqueCarbonFootprintNames, responsibleSubjects);
    }
  }
}