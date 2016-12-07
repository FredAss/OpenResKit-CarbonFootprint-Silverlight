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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Ork.Framework.CarbonFootprints.Commands;
using Ork.Framework.CarbonFootprints.Interfaces;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.CarbonFootprints.ViewModels;
using Ork.Framework.DomainModelService;
using Ork.Framework.Framework;
using Ork.Framework.Settings;

namespace Ork.Framework.CarbonFootprints
{
  public class CarbonFootprintViewModel : DocumentBase
  {
    private readonly ContextRepository m_ContextRepository;
    private readonly ObservableCollection<PositionViewModel> m_Positions = new ObservableCollection<PositionViewModel>();
    private readonly IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;
    private readonly TagColorProvider m_TagColorProvider;
    private readonly string[] m_UniqueCarbonFootprintNames;
    private bool m_IsCfpChoiceVisible;
    private bool m_IsSelected;
    private PositionViewModel m_SelectedPosition;

    public CarbonFootprintViewModel(CarbonFootprint cf, IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> factories, ContextRepository contextRepository,
      string[] uniqueCarbonFootprintNames, TagColorProvider tagColorProvider, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      PositionFactories = factories;
      m_ResponsibleSubjects = responsibleSubjects;
      m_ContextRepository = contextRepository;
      Model = cf;
      m_UniqueCarbonFootprintNames = uniqueCarbonFootprintNames;
      m_TagColorProvider = tagColorProvider;
      m_IsSelected = true;
      InitializePositions(Model);

      RemoveCommand = new RelayCommand(Remove);
    }

    public ObservableCollection<TagColor> TagColors
    {
      get { return m_TagColorProvider.TagColors; }
    }

    public PositionViewModel ActiveItem { get; private set; }

    public double CalculatedValue
    {
      get { return Model.Calculation; }
    }

    public string Calculation
    {
      get
      {
        var kgvalue = String.Format("{0:0.##}", Model.Calculation / 1000000);
        return kgvalue;
      }
    }

    public IEnumerable<PieChartDataPointViewModel> ChartData
    {
      get { return GetChartData(); }
    }

    public string Description
    {
      get { return Model.Description; }
      set
      {
        Model.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }

    public IEnumerable<PieChartDataPointViewModel> DetailedChartData
    {
      get { return GetDetailedChartData(); }
    }

    public bool IsCfpChoiceVisible
    {
      get { return m_IsCfpChoiceVisible; }
      set
      {
        if (value.Equals(m_IsCfpChoiceVisible))
        {
          return;
        }
        m_IsCfpChoiceVisible = value;
        NotifyOfPropertyChange(() => IsCfpChoiceVisible);
      }
    }

    public bool IsSelected
    {
      get { return m_IsSelected; }
      set
      {
        if (m_IsSelected == value)
        {
          return;
        }
        m_IsSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
        if (IsSelectedChanged != null)
        {
          IsSelectedChanged(this, new EventArgs());
        }
      }
    }

    public CarbonFootprint Model { get; private set; }

    public string Name
    {
      get { return Model.Name; }
      set
      {
        if (m_UniqueCarbonFootprintNames.Contains(value))
        {
          return;
        }

        Model.Name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }

    public IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> PositionFactories { get; private set; }

    public ObservableCollection<PositionViewModel> Positions
    {
      get { return m_Positions; }
    }

    public ICommand RemoveCommand { get; private set; }

    public PositionViewModel SelectedPosition
    {
      get { return m_SelectedPosition; }
      set
      {
        if (m_SelectedPosition == value)
        {
          return;
        }
        m_SelectedPosition = value;
        NotifyOfPropertyChange(() => SelectedPosition);
      }
    }

    public event EventHandler IsSelectedChanged;

    public void AddPosition(Lazy<IPositionViewModelFactory, IPositionMetadata> factory)
    {
      var newCfp = factory.Value.CreatePredefinedPosition(m_ResponsibleSubjects);
      AddCarbonFootprintPosition(newCfp);
    }

    public IEnumerable<PieChartDataPointViewModel> GetChartData()
    {
      var categories = Positions.Select(p => p.Model.Tag)
                                .Distinct();
      var values = new Collection<PieChartDataPointViewModel>();
      foreach (var category in categories)
      {
        var matchingPositionSum = Positions.Where(cfp => cfp.Model.Tag == category)
                                           .Sum(cfp => cfp.CalculatedValue);
        values.Add(new PieChartDataPointViewModel(category, matchingPositionSum, m_TagColorProvider.GetColorForTag));
      }
      return values;
    }

    public void Remove(object item)
    {
      var positionViewModel = item as PositionViewModel;

      if (positionViewModel == null)
      {
        return;
      }

      IsDirty = true;
      Model.Positions.Remove(positionViewModel.Model);
      Positions.Remove(positionViewModel);
    }

    public void Save()
    {
      m_ContextRepository.SaveCompleted += SaveCompleted;
      m_ContextRepository.Save();
    }

    private void AddCarbonFootprintPosition(PositionViewModel position)
    {
      IsDirty = true;
      position.Start = DateTime.Today;
      position.Finish = DateTime.Today.AddYears(1);
      Model.Positions.Add(position.Model);
      Positions.Add(position);
      SelectedPosition = position;
      IsCfpChoiceVisible = !IsCfpChoiceVisible;
    }

    private IEnumerable<PieChartDataPointViewModel> GetDetailedChartData()
    {
      var categories = Positions.Select(p => p.Model.Tag)
                                .Distinct();
      var values = new Collection<PieChartDataPointViewModel>();
      foreach (var category in categories)
      {
        var matchingPositionSum = Positions.Where(cfp => cfp.Model.Tag == category)
                                           .Sum(cfp => cfp.CalculatedValue);
        values.Add(new PieChartDataPointViewModel(category, matchingPositionSum, m_TagColorProvider.GetColorForTag));
      }
      return values;
    }

    private void InitializePositions(CarbonFootprint cf)
    {
      var factories = PositionFactories.Select(pf => pf.Value)
                                       .ToArray();
      foreach (var carbonFootprintPosition in cf.Positions)
      {
        foreach (var positionFactory in factories.Where(positionFactory => positionFactory.CanDecorate(carbonFootprintPosition)))
        {
          m_Positions.Add(positionFactory.CreateFromExisting(carbonFootprintPosition, m_ResponsibleSubjects));
        }
      }
    }

    private void SaveCompleted(object sender, EventArgs e)
    {
      m_ContextRepository.SaveCompleted -= SaveCompleted;
      m_ContextRepository.Calculate(Model.Id);
    }
  }
}