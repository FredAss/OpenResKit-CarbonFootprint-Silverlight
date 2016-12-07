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

using System.Collections;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;

namespace Ork.Framework.CarbonFootprints.Controls
{
  public class BindableStackedColumnSeries : StackedColumnSeries
  {
    //private readonly CollectionChangedWeakEventSource m_SourceChanged;
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable), typeof (BindableStackedColumnSeries),
      new PropertyMetadata(default(IEnumerable), OnSeriesSourceChanged));


    //public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style",
    //																																											typeof (Style),
    //																																											typeof (BindableStackedColumnSeries),
    //																																											new PropertyMetadata(new Style()));

    public IEnumerable ItemsSource
    {
      get { return (IEnumerable) GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    //public Style Style { get { return (Style) GetValue(StyleProperty); } set { SetValue(StyleProperty, value); } }

    protected virtual void UpdateSeriesDefinitions(IEnumerable oldValue, IEnumerable newValue)
    {
      //m_SourceChanged.SetEventSource(newValue);
      SeriesDefinitions.Clear();

      if (newValue != null)
      {
        foreach (var item in newValue)
        {
          SeriesDefinitions.Add((SeriesDefinition) item);
        }
      }
    }

    private static void OnSeriesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var oldValue = (IEnumerable) e.OldValue;
      var newValue = (IEnumerable) e.NewValue;
      var bindableSeries = (BindableStackedColumnSeries) d;
      bindableSeries.UpdateSeriesDefinitions(oldValue, newValue);
    }

    //void SourceCollectionChangedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //{

    //	switch (e.Action)
    //	{
    //		case NotifyCollectionChangedAction.Add:

    //			if (e.NewItems != null)
    //			{
    //				for (int i = 0; i < e.NewItems.Count; i++)
    //				{
    //					CreateSeries(e.NewItems[0], e.NewStartingIndex + i);
    //				}
    //			}

    //			break;
    //		case NotifyCollectionChangedAction.Remove:

    //			if (e.OldItems != null)
    //			{
    //				for (int i = e.OldItems.Count; i > 0; i--)
    //				{
    //					this.Series.RemoveAt(i + e.OldStartingIndex - 1);
    //				}
    //			}

    //			break;
    //		case NotifyCollectionChangedAction.Replace:
    //			throw new NotImplementedException("NotifyCollectionChangedAction.Replace is not implemented by MultiChart control.");

    //		case NotifyCollectionChangedAction.Reset:

    //			this.Series.Clear();

    //			if (SeriesSource != null)
    //			{
    //				foreach (object item in SeriesSource)
    //				{
    //					CreateSeries(item);
    //				}
    //			}

    //			break;
    //		default:
    //			break;
    //	}
  }
}