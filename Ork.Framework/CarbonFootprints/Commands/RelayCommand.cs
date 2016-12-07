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
using System.Windows.Input;

namespace Ork.Framework.CarbonFootprints.Commands
{
  public class RelayCommand : ICommand
  {
    private readonly Predicate<object> m_CanExecute;
    private readonly Action<object> m_Execute;

    public RelayCommand(Action<object> execute)
      : this(execute, (o => true))
    {
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      if (execute == null)
      {
        throw new ArgumentNullException("execute");
      }

      m_Execute = execute;
      m_CanExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      return m_CanExecute(parameter);
    }

    public void Execute(object parameter)
    {
      m_Execute(parameter);
    }
  }
}