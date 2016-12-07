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

using System.Windows.Media;
using Caliburn.Micro;

namespace Ork.Framework.Settings
{
  public class TagColor : PropertyChangedBase
  {
    private Color m_Color;

    public TagColor()
    {
      //has to stay here
    }

    public TagColor(string tag, Color color)
    {
      Tag = tag;
      Color = color;
    }

    public Color Color
    {
      get { return m_Color; }
      set
      {
        if (value.Equals(m_Color))
        {
          return;
        }
        m_Color = value;
        NotifyOfPropertyChange(() => Color);
        NotifyOfPropertyChange(() => ColorBrush);
      }
    }

    public SolidColorBrush ColorBrush
    {
      get { return new SolidColorBrush(Color); }
    }

    public string Tag { get; set; }
  }
}