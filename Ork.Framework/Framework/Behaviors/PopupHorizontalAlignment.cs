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
  public enum PopupHorizontalAlignment
  {
    // the left side of the popup is aligned with the left side of the placement target
    Left,
    // the right side of the popup is aligned with the center of the placement target
    RightCenter,
    // the center of the popup is aligned with the center of the placement target
    Center,
    // the left side of the popup is aligned with the center of the placement target
    LeftCenter,
    // the right side of the popup is aligned with the right side of the placement target
    Right
  }
}