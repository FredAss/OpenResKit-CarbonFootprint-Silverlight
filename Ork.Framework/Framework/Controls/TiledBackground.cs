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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ork.Framework.Framework.Controls
{
  /// <summary>
  ///   This class was borrowed from the SL4 JetPack Theme.
  /// </summary>
  public class TiledBackground : UserControl
  {
    public static readonly DependencyProperty SourceUriProperty = DependencyProperty.Register("SourceUri", typeof (Uri), typeof (TiledBackground), new PropertyMetadata(null, OnSourceUriChanged));
    private readonly Image tiledImage = new Image();

    private BitmapImage bitmap;
    private int lastHeight;
    private int lastWidth;
    private WriteableBitmap sourceBitmap;

    public TiledBackground()
    {
      tiledImage.Stretch = Stretch.None;
      Content = tiledImage;
      SizeChanged += TiledBackgroundSizeChanged;
    }

    /// <summary>
    ///   A description of the property.
    /// </summary>
    public Uri SourceUri
    {
      get { return (Uri) GetValue(SourceUriProperty); }
      set { SetValue(SourceUriProperty, value); }
    }

    private void TiledBackgroundSizeChanged(object sender, SizeChangedEventArgs e)
    {
      UpdateTiledImage();
    }

    private void UpdateTiledImage()
    {
      if (sourceBitmap == null)
      {
        return;
      }

      var width = (int) Math.Ceiling(ActualWidth);
      var height = (int) Math.Ceiling(ActualHeight);

      // only regenerate the image if the width/height has grown
      if (width < lastWidth &&
          height < lastHeight)
      {
        return;
      }
      lastWidth = width;
      lastHeight = height;

      var final = new WriteableBitmap(width, height);

      for (var x = 0; x < final.PixelWidth; x++)
      {
        for (var y = 0; y < final.PixelHeight; y++)
        {
          var tiledX = (x % sourceBitmap.PixelWidth);
          var tiledY = (y % sourceBitmap.PixelHeight);
          final.Pixels[y * final.PixelWidth + x] = sourceBitmap.Pixels[tiledY * sourceBitmap.PixelWidth + tiledX];
        }
      }

      tiledImage.Source = final;
    }

    private static void OnSourceUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((TiledBackground) d).OnSourceUriChanged(e);
    }

    protected virtual void OnSourceUriChanged(DependencyPropertyChangedEventArgs e)
    {
      bitmap = new BitmapImage(e.NewValue as Uri)
               {
                 CreateOptions = BitmapCreateOptions.None
               };
      bitmap.ImageOpened += BitmapImageOpened;
    }

    private void BitmapImageOpened(object sender, RoutedEventArgs e)
    {
      sourceBitmap = new WriteableBitmap(bitmap);
      UpdateTiledImage();
    }
  }
}