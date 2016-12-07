﻿#region License

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
using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace Ork.Framework.Framework
{
  public class DocumentBase : Screen, IHaveShutdownTask
  {
    private bool isDirty;

    public bool IsDirty
    {
      get { return isDirty; }
      set
      {
        isDirty = value;
        NotifyOfPropertyChange(() => IsDirty);
      }
    }

    [Import]
    public IDialogManager Dialogs { get; set; }

    public IResult GetShutdownTask()
    {
      return IsDirty
        ? new ApplicationCloseCheck(this, DoCloseCheck)
        : null;
    }

    public override void CanClose(Action<bool> callback)
    {
      if (IsDirty)
      {
        DoCloseCheck(Dialogs, callback);
      }
      else
      {
        callback(true);
      }
    }

    protected virtual void DoCloseCheck(IDialogManager dialogs, Action<bool> callback)
    {
      dialogs.ShowMessageBox("Die vorgenommenen Änderungen wurden noch nicht gespeichert. Wollen Sie die Anwendung wirklich beenden? Alle Änderungen gehen dabei verloren.", "Warnung",
        MessageBoxOptions.YesNo, box => callback(box.WasSelected(MessageBoxOptions.Yes)));
    }
  }
}