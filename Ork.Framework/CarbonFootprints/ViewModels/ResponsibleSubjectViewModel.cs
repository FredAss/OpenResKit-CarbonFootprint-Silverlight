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

using Ork.Framework.DomainModelService;

namespace Ork.Framework.CarbonFootprints.ViewModels
{
  public class ResponsibleSubjectViewModel
  {
    public ResponsibleSubjectViewModel(ResponsibleSubject model)
    {
      Model = model;
    }

    public ResponsibleSubject Model { get; private set; }

    public string Name
    {
      get
      {
        if (Model is Employee)
        {
          var employee = (Employee) Model;
          return string.Format("{0} {1}", employee.FirstName, employee.LastName);
        }
        else
        {
          var employeeGroup = (EmployeeGroup) Model;
          return string.Format("{0}", employeeGroup.Name);
        }
      }
    }
  }
}