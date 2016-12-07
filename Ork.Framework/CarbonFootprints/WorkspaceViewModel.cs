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
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Ork.Framework.CarbonFootprints.Commands;
using Ork.Framework.CarbonFootprints.Factories;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.CarbonFootprints.ViewModels;
using Ork.Framework.DomainModelService;
using Ork.Framework.Framework;
using Ork.Framework.Settings;

namespace Ork.Framework.CarbonFootprints
{
  [Export(typeof (IWorkspace))]
  public class WorkspaceViewModel : DocumentWorkspace<CarbonFootprintViewModel>
  {
    private readonly CarbonFootprintViewModelFactory m_CarbonFootprintViewModelFactory;
    private readonly ContextRepository m_Repository;
    private readonly TagColorProvider m_TagProvider;
    private Visibility m_RefreshStateVisibility;
    private IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;

    [ImportingConstructor]
    public WorkspaceViewModel(CarbonFootprintViewModelFactory carbonFootprintViewModelFactory, ContextRepository repository, TagColorProvider tagProvider)
    {
      DisplayName = "Carbon Footprints";
      m_CarbonFootprintViewModelFactory = carbonFootprintViewModelFactory;
      m_Repository = repository;
      m_TagProvider = tagProvider;

      m_Repository.SaveCompleted += SaveCompleted;
      m_Repository.ContextChanged += (s, e) => LoadData();

      RefreshStateVisibility = Visibility.Collapsed;

      EditCommand = new RelayCommand(x => Edit((CarbonFootprintViewModel) x));
      RemoveCommand = new RelayCommand(Remove);
    }

    public ICommand EditCommand { get; private set; }

    public Visibility RefreshStateVisibility
    {
      get { return m_RefreshStateVisibility; }
      set
      {
        if (m_RefreshStateVisibility == value)
        {
          return;
        }
        m_RefreshStateVisibility = value;
        NotifyOfPropertyChange(() => RefreshStateVisibility);
      }
    }

    public ICommand RemoveCommand { get; private set; }

    public IEnumerable<SeriesDefinition> SeriesDefinitions
    {
      get { return GetSeriesDefinitions(); }
    }

    public ObservableCollection<TagColor> TagColors
    {
      get { return m_TagProvider.TagColors; }
    }

    public void New()
    {
      var serialNumber = m_Repository.CarbonFootprints.Count + 1;
      var vm = m_CarbonFootprintViewModelFactory.CreateNew(String.Format("Neuer Carbon Footprint {0}", serialNumber), string.Empty, 0, string.Empty, m_Repository.CarbonFootprints.Select(n => n.Name)
                                                                                                                                                                 .ToArray(), m_ResponsibleSubjects);
      m_Repository.CarbonFootprints.Add(vm.Model);
      vm.IsDirty = true;
      Edit(vm);
    }

    public void Remove(object item)
    {
      var carbonFootprint = item as CarbonFootprintViewModel;

      if (carbonFootprint == null)
      {
        return;
      }

      m_Repository.CarbonFootprints.Remove(carbonFootprint.Model);
      Items.Remove(carbonFootprint);

      m_Repository.Save();

      NotifyOfPropertyChange(() => SeriesDefinitions);
    }

    //private void CallDialog(string message, string caption)
    //{
    //  if (Parent is ShellViewModel)
    //  {
    //    ((ShellViewModel) Parent).Dialogs.ShowMessageBox(message, caption);
    //  }
    //}

    private Style GetChartStyleForTag()
    {
      var style = new Style(typeof (ColumnDataPoint));

      var backgroundSetter = new Setter();
      var borderThicknessSetter = new Setter();

      var binding = new Binding();
      binding.Path = new PropertyPath("TagColor.ColorBrush");
      backgroundSetter.Property = Control.BackgroundProperty;
      backgroundSetter.Value = binding;

      borderThicknessSetter.Property = Control.BorderThicknessProperty;
      borderThicknessSetter.Value = new Thickness(0);

      style.Setters.Add(backgroundSetter);
      style.Setters.Add(borderThicknessSetter);

      var cellXaml = @"<ControlTemplate
          xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
          xmlns:toolkit=""http://schemas.microsoft.com/winfx/2006/xaml/presentation/toolkit""
          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
          xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
          TargetType=""toolkit:ColumnDataPoint"">
          <Rectangle
            Fill=""{TemplateBinding Background}"">
            <ToolTipService.ToolTip>
              <StackPanel Margin=""2,2,2,2"">
                <TextBlock Text=""{Binding Tag}"" />
                <TextBlock DataContext=""{TemplateBinding DependentValue}""
                           Text=""{Binding StringFormat='{}{0:0.00} kg CO2' }""></TextBlock>
                  </StackPanel>
                </ToolTipService.ToolTip>
          </Rectangle>
        </ControlTemplate>";
      var cellTemplate = XamlReader.Load(cellXaml) as ControlTemplate;

      var templateSetter = new Setter
                           {
                             Property = Control.TemplateProperty,
                             Value = cellTemplate
                           };
      style.Setters.Add(templateSetter);
      return style;
    }

    private IEnumerable<SeriesDefinition> GetSeriesDefinitions()
    {
      if (Items == null)
      {
        yield break;
      }
      var categories = Items.SelectMany(p => p.Model.Positions)
                            .Select(p => p.Tag)
                            .Distinct()
                            .OrderBy(c => c)
                            .ToArray();

      var dict = new Dictionary<CarbonFootprint, PieChartDataPointViewModel[]>();
      foreach (var carbonFootprintViewModel in Items.Where(vm => vm.IsSelected))
      {
        var data = carbonFootprintViewModel.GetChartData();
        var missingCategories = categories.Except(data.Select(d => d.Category));
        foreach (var category in missingCategories)
        {
          data = data.Concat(new[]
                             {
                               new PieChartDataPointViewModel(category, 0, m_TagProvider.GetColorForTag)
                             });
        }
        data = data.OrderBy(d => d.Category);
        dict.Add(carbonFootprintViewModel.Model, data.ToArray());
      }

      List<StackChartDataPointViewModel> sumPerCFAndCategory;
      //string lastCategory = string.Empty;
      for (var i = 0; i < categories.Count(); i++)
      {
        sumPerCFAndCategory = dict.Select(entry => new StackChartDataPointViewModel(entry.Key.Name, entry.Value[i].Sum / 1000, categories[i], m_TagProvider.GetColorForTag))
                                  .ToList();

        var sd = new SeriesDefinition();
        sd.ItemsSource = sumPerCFAndCategory;
        sd.Title = categories[i];
        sd.DataPointStyle = GetChartStyleForTag();
        sd.IndependentValuePath = "Key";
        sd.DependentValuePath = "Sum";

        yield return sd;
      }
    }

    //private void LoadCarbonFootprints(DomainModelContext context)
    //{
    //  try
    //  {
    //    //int? previousEditedModelId = null;
    //    if (State == DocumentWorkspaceState.CarbonFootprintDetailView)
    //    {
    //      //previousEditedModelId = ActiveItem.Model.Id;
    //      DeactivateItem(ActiveItem, true);
    //    }

    //    var collection = new DataServiceCollection<CarbonFootprint>(m_Repository.Context);

    //    Items.Clear();
    //    collection.LoadCompleted += delegate(object s, LoadCompletedEventArgs e)
    //                                {
    //                                  try
    //                                  {
    //                                    if (e.Error != null)
    //                                    {
    //                                      throw e.Error;
    //                                    }

    //                                    if (collection.Continuation != null)
    //                                    {
    //                                      collection.LoadNextPartialSetAsync();
    //                                    }
    //                                    m_CarbonFootprints = collection;

    //                                    Items.AddRange(collection.Select(cf => m_CarbonFootprintViewModelFactory.CreateFromExisting(cf, m_CarbonFootprints.Select(n => n.Name)
    //                                                                                                                                                      .ToArray(), m_ResponsibleSubjects)));

    //                                    m_HasValidConnection = true;
    //                                    RefreshStateVisibility = Visibility.Collapsed;

    //                                    AddSelectionListener();

    //                                    m_TagProvider.Initialize(m_CarbonFootprints.SelectMany(cf => cf.Positions)
    //                                                                               .Select(cfp => cfp.Tag));

    //                                    NotifyOfPropertyChange(() => SeriesDefinitions);
    //                                  }
    //                                  catch (Exception innerEx)
    //                                  {
    //                                    RefreshStateVisibility = Visibility.Visible;
    //                                    CallDialog(innerEx.ToString(), "Fehler");
    //                                  }
    //                                };

    //    var query = context.CarbonFootprints.Expand("Positions")
    //                       .Expand("Positions/OpenResKit.DomainModel.CarbonFootprintPosition/ResponsibleSubject")
    //                       .Expand("Positions/OpenResKit.DomainModel.AirportBasedFlight/Airports");
    //    collection.LoadAsync(query);
    //  }
    //  catch (Exception outerEx)
    //  {
    //    CallDialog(outerEx.Message, "Fehler");
    //  }
    //}

    //private void LoadResponsibleSubjects(DomainModelContext context)
    //{
    //  try
    //  {
    //    var collection = new DataServiceCollection<ResponsibleSubject>(m_Repository.Context);

    //    collection.LoadCompleted += delegate(object s, LoadCompletedEventArgs e)
    //                                {
    //                                  try
    //                                  {
    //                                    if (e.Error != null)
    //                                    {
    //                                      throw e.Error;
    //                                    }
    //                                    if (collection.Continuation != null)
    //                                    {
    //                                      collection.LoadNextPartialSetAsync();
    //                                    }
    //                                    m_ResponsibleSubjects = collection.Select(rs => new ResponsibleSubjectViewModel(rs))
    //                                                                      .ToArray();
    //                                    LoadCarbonFootprints(context);
    //                                  }
    //                                  catch (Exception innerEx)
    //                                  {
    //                                    RefreshStateVisibility = Visibility.Visible;
    //                                    CallDialog(innerEx.ToString(), "Fehler");
    //                                  }
    //                                };

    //    var query = context.ResponsibleSubjects;
    //    collection.LoadAsync(query);
    //  }
    //  catch (Exception outerEx)
    //  {
    //    CallDialog(outerEx.Message, "Fehler");
    //  }
    //}

    private void LoadData()
    {
      if (State == DocumentWorkspaceState.CarbonFootprintDetailView)
      {
        DeactivateItem(ActiveItem, true);
      }

      Items.Clear();
      LoadResponsibleSubjects();
      LoadCarbonFootprints();

      RefreshStateVisibility = Visibility.Collapsed;
      m_TagProvider.Initialize(m_Repository.CarbonFootprints.SelectMany(cf => cf.Positions)
                                           .Select(cfp => cfp.Tag));
      NotifyOfPropertyChange(() => SeriesDefinitions);
    }

    private void LoadResponsibleSubjects()
    {
      m_ResponsibleSubjects = m_Repository.ResponsibleSubjects.Select(rs => new ResponsibleSubjectViewModel(rs))
                                          .ToArray();
    }

    private void LoadCarbonFootprints()
    {
      Items.AddRange(m_Repository.CarbonFootprints.Select(cf => m_CarbonFootprintViewModelFactory.CreateFromExisting(cf, m_Repository.CarbonFootprints.Select(n => n.Name)
                                                                                                                                     .ToArray(), m_ResponsibleSubjects))
                                 .ToArray());
      foreach (var carbonFootprintViewModel in Items)
      {
        carbonFootprintViewModel.IsSelectedChanged += (s, e) => NotifyOfPropertyChange(() => SeriesDefinitions);
      }
    }

    private void SaveCompleted(object sender, EventArgs e)
    {
      foreach (var carbonFootprintViewModel in Items)
      {
        carbonFootprintViewModel.IsDirty = false;
      }
    }
  }
}