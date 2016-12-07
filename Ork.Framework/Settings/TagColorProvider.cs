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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace Ork.Framework.Settings
{
  [Export]
  public class TagColorProvider
  {
    private const string FileName = "TagColors.xml";
    private Random m_Random;

    [ImportingConstructor]
    public TagColorProvider()
    {
      m_Random = new Random();
      TagColors = new ObservableCollection<TagColor>();
    }

    public ObservableCollection<TagColor> TagColors { get; private set; }

    [Export]
    public TagColor GetColorForTag(string tag)
    {
      var tagColor = TagColors.SingleOrDefault(tc => tc.Tag == tag);
      if (tagColor != null)
      {
        return tagColor;
      }
      else
      {
        var color = AddNewColorForTag(tag);
        SaveColorsToXml();
        return color;
      }
    }


    public void SaveColorsToXml()
    {
      // Write to the Isolated Storage
      var xmlWriterSettings = new XmlWriterSettings();
      xmlWriterSettings.Indent = true;

      using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
      {
        using (var stream = myIsolatedStorage.OpenFile(FileName, FileMode.Create))
        {
          var serializer = new XmlSerializer(typeof (List<TagColor>));
          using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
          {
            serializer.Serialize(xmlWriter, TagColors.ToList());
          }
        }
      }
    }

    public void Initialize(IEnumerable<string> tags)
    {
      TagColors.Clear();

      tags = tags.Distinct();
      var tagColorsFromXml = ReadColorsFromXml()
        .Where(tc => tags.Contains(tc.Tag));

      foreach (var tag in tags)
      {
        var savedTagColor = tagColorsFromXml.SingleOrDefault(c => c.Tag == tag);
        if (savedTagColor != null)
        {
          TagColors.Add(savedTagColor);
        }
        else
        {
          AddNewColorForTag(tag);
        }
      }
      SaveColorsToXml();
    }

    private TagColor AddNewColorForTag(string tag)
    {
      var colorBytes = new byte[3];
      m_Random.NextBytes(colorBytes);
      var tagColor = new TagColor(tag, Color.FromArgb(255, colorBytes[0], colorBytes[1], colorBytes[2]));
      TagColors.Add(tagColor);
      return tagColor;
    }

    private IEnumerable<TagColor> ReadColorsFromXml()
    {
      using (var myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
      {
        if (myIsolatedStorage.FileExists(FileName))
        {
          using (var stream = myIsolatedStorage.OpenFile(FileName, FileMode.Open))
          {
            var serializer = new XmlSerializer(typeof (List<TagColor>));
            var data = (List<TagColor>) serializer.Deserialize(stream);
            return data;
          }
        }
        else
        {
          return new List<TagColor>();
        }
      }
    }
  }
}