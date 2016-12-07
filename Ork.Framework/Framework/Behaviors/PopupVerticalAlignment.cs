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

namespace Ork.Framework.Framework.Behaviors
{
  public enum PopupVerticalAlignment
  {
    // the top side of the popup is aligned with the top side of the placement target
    Top,
    // the bottom side of the popup is aligned with the center of the placement target
    BottomCenter,
    // the center of the popup is aligned with the center of the placement target
    Center,
    // the top side of the popup is aligned with the center of the placement target
    TopCenter,
    // the bottom side of the popup is aligned with the bottom side of the placement target
    Bottom
  }
}