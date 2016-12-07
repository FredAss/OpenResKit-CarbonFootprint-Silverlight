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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Ork.Framework.CarbonFootprints;
using Ork.Framework.Framework;

namespace Ork.Framework.Shell
{
  public class ScreensBootstrapper : Bootstrapper<IShell>
  {
    private bool actuallyClosing;
    private CompositionContainer container;
    private Window mainWindow;

    protected override void Configure()
    {
      container = CompositionHost.Initialize(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x))
                                                                                .OfType<ComposablePartCatalog>()));

      var batch = new CompositionBatch();

      // manually extend the MEF container with caliburn types

      batch.AddExportedValue<IWindowManager>(new WindowManager());
      batch.AddExportedValue<IEventAggregator>(new EventAggregator());

      batch.AddExportedValue<Func<IMessageBox>>(() => container.GetExportedValue<IMessageBox>());
      batch.AddExportedValue<Func<CarbonFootprintViewModel>>(() => container.GetExportedValue<CarbonFootprintViewModel>());
      batch.AddExportedValue(container);

      container.Compose(batch);
    }


    protected override object GetInstance(Type serviceType, string key)
    {
      var contract = string.IsNullOrEmpty(key)
        ? AttributedModelServices.GetContractName(serviceType)
        : key;
      var exports = container.GetExportedValues<object>(contract);

      if (exports.Count() > 0)
      {
        return exports.First();
      }

      throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
    }

    protected override IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
    }

    protected override void BuildUp(object instance)
    {
      container.SatisfyImportsOnce(instance);
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
      base.OnStartup(sender, e);

      if (Application.IsRunningOutOfBrowser)
      {
        mainWindow = Application.MainWindow;
        mainWindow.Closing += MainWindowClosing;
      }
    }

    private void MainWindowClosing(object sender, ClosingEventArgs e)
    {
      if (actuallyClosing)
      {
        return;
      }

      e.Cancel = true;

      Execute.OnUIThread(() =>
                         {
                           var shell = IoC.Get<IShell>();

                           shell.CanClose(result =>
                                          {
                                            if (result)
                                            {
                                              actuallyClosing = true;
                                              mainWindow.Close();
                                            }
                                          });
                         });
    }
  }
}