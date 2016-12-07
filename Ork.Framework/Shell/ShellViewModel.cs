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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Ork.Framework.Framework;

namespace Ork.Framework.Shell
{
  [Export(typeof (IShell))]
  public class ShellViewModel : Conductor<IWorkspace>.Collection.OneActive, IShell
  {
    private readonly IDialogManager dialogs;

    [ImportingConstructor]
    public ShellViewModel(IDialogManager dialogs, [ImportMany] IEnumerable<IWorkspace> workspaces)
    {
      this.dialogs = dialogs;

      Items.AddRange(workspaces);

      if (workspaces.Any())
      {
        workspaces.First()
                  .Show();
      }


      CloseStrategy = new ApplicationCloseStrategy();
    }

    public IDialogManager Dialogs
    {
      get { return dialogs; }
    }
  }
}