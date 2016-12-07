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
using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace Ork.Framework.Framework
{
  public class ApplicationCloseCheck : IResult
  {
    private readonly Action<IDialogManager, Action<bool>> closeCheck;
    private readonly IChild screen;

    public ApplicationCloseCheck(IChild screen, Action<IDialogManager, Action<bool>> closeCheck)
    {
      this.screen = screen;
      this.closeCheck = closeCheck;
    }

    [Import]
    public IShell Shell { get; set; }

    public void Execute(ActionExecutionContext context)
    {
      var documentWorkspace = screen.Parent as IDocumentWorkspace;
      if (documentWorkspace != null)
      {
        documentWorkspace.Edit(screen);
      }

      closeCheck(Shell.Dialogs, result => Completed(this, new ResultCompletionEventArgs
                                                          {
                                                            WasCancelled = !result
                                                          }));
    }

    public event EventHandler<ResultCompletionEventArgs> Completed = delegate
                                                                     {
                                                                     };
  }
}