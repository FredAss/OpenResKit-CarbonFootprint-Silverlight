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

using System.ComponentModel;
using Caliburn.Micro;

namespace Ork.Framework.Framework
{
  public abstract class DocumentWorkspace<TDocument> : Conductor<TDocument>.Collection.OneActive, IDocumentWorkspace
    where TDocument : class, INotifyPropertyChanged, IDeactivate, IHaveDisplayName
  {
    private DocumentWorkspaceState state = DocumentWorkspaceState.CarbonFootprintOverView;

    protected DocumentWorkspace()
    {
      Items.CollectionChanged += delegate
                                 {
                                   NotifyOfPropertyChange(() => Status);
                                 };
    }

    public DocumentWorkspaceState State
    {
      get { return state; }
      set
      {
        if (state == value)
        {
          return;
        }

        state = value;
        NotifyOfPropertyChange(() => State);
      }
    }

    protected IConductor Conductor
    {
      get { return (IConductor) Parent; }
    }

    public string Status
    {
      get
      {
        return Items.Count > 0
          ? Items.Count.ToString()
          : string.Empty;
      }
    }

    public void Show()
    {
      var haveActive = Parent as IHaveActiveItem;
      if (haveActive != null &&
          haveActive.ActiveItem == this)
      {
        State = DocumentWorkspaceState.CarbonFootprintOverView;
      }
      else
      {
        Conductor.ActivateItem(this);
      }
    }

    void IDocumentWorkspace.Edit(object document)
    {
      Edit((TDocument) document);
    }

    public void Edit(TDocument child)
    {
      if (child == null)
      {
        return;
      }
      Conductor.ActivateItem(this);
      State = DocumentWorkspaceState.CarbonFootprintDetailView;
      ActivateItem(child);
    }

    public override void ActivateItem(TDocument item)
    {
      if (item == null)
      {
        return;
      }
      item.Deactivated += OnItemOnDeactivated;
      item.PropertyChanged += OnItemPropertyChanged;

      base.ActivateItem(item);
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "DisplayName")
      {
        DisplayName = ((TDocument) sender).DisplayName;
      }
    }

    private void OnItemOnDeactivated(object sender, DeactivationEventArgs e)
    {
      var doc = (TDocument) sender;
      if (e.WasClosed)
      {
        State = DocumentWorkspaceState.CarbonFootprintOverView;
        doc.Deactivated -= OnItemOnDeactivated;
        doc.PropertyChanged -= OnItemPropertyChanged;
      }
    }
  }
}