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
using System.ComponentModel.Composition;
using Ork.Framework.CarbonFootprints.Interfaces;
using Ork.Framework.CarbonFootprints.Model;
using Ork.Framework.CarbonFootprints.ViewModels;
using Ork.Framework.DomainModelService;
using Ork.Framework.Settings;

namespace Ork.Framework.CarbonFootprints.Factories
{
  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Flug", "Einen neuen Flug erstellen.", "../CarbonFootprints/Resources/Images/flight.png")]
  internal class FlightFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    internal FlightFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (Flight);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new FlightViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateFlight(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreateFlight("Flug", "Stellt einen Flug mit manuellen Eingaben dar.", "Flüge", "CfFlight.png", DateTime.Now, DateTime.Now.AddYears(1), 3000, FlightViewModel.FlightRange.Mittelstrecke),
          responsibleSubjects);
    }

    private Flight CreateFlight(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0,
      FlightViewModel.FlightRange flightRange = FlightViewModel.FlightRange.Kurzstrecke, bool radiativeForcing = false)
    {
      return new Flight
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_FlightType = (int) flightRange,
               Kilometrage = kilometrage,
               RadiativeForcing = radiativeForcing
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Autofahrt", "Eine neue Autofahrt erstellen.", "../CarbonFootprints/Resources/Images/car.png")]
  internal class CarFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public CarFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (Car);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new CarViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateCar(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreateCar("Fahrzeug", "Stellt ein Fahrzeug für die individuelle Personenbeförderung dar.", "Fahrzeuge", "CfCar.png", DateTime.Now, DateTime.Now.AddYears(1), 2000, 10.2, 120,
            CarViewModel.Fuel.Benzin), responsibleSubjects);
    }

    private Car CreateCar(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0, double consumption = 0, double carbonProduction = 0,
      CarViewModel.Fuel fuelType = CarViewModel.Fuel.Diesel)
    {
      return new Car
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Kilometrage = kilometrage,
               CarbonProduction = carbonProduction,
               m_Fuel = (int) fuelType,
               Consumption = consumption
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Flugziele", "Einen neuen zielbasierten Flug erstellen.", "../CarbonFootprints/Resources/Images/flight.png")]
  public class AirportBasedFlightFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public AirportBasedFlightFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (AirportBasedFlight);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new AirportBasedFlightViewModel(model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateAirportBasedFlight(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(CreateAirportBasedFlight("Linienflug", "Stellt eine Flugkonfiguration basierend auf Linienflügen bereit.", "Flüge", "CfFlight.png", DateTime.Now, DateTime.Now.AddYears(1)),
          responsibleSubjects);
    }

    private AirportBasedFlight CreateAirportBasedFlight(string name, string description, string tag, string iconId, DateTime start, DateTime finish)
    {
      return new AirportBasedFlight
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Spezifisches Auto", "Einen neuen spezifisches Auto erstellen.", "../CarbonFootprints/Resources/Images/car.png")]
  internal class FullyQualifiedCarFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public FullyQualifiedCarFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (FullyQualifiedCar);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new FullyQualifiedCarViewModel(model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateFullyQualifiedCar(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreateFullyQualifiedCar("Herstellerfahrzeug", "Stellt ein reales Fahrzeug eines konkreten Herstellers dar.", "Fahrzeuge", "CfCar.png", DateTime.Now, DateTime.Now.AddYears(1), 100, 13),
          responsibleSubjects);
    }

    private FullyQualifiedCar CreateFullyQualifiedCar(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0, int carId = 1,
      double consumption = 0)
    {
      return new FullyQualifiedCar
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Kilometrage = kilometrage,
               Consumption = consumption,
               CarId = carId
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Energieverbrauch", "Einen neuen Energieverbrauch erstellen.", "../CarbonFootprints/Resources/Images/energyconsumption.png")]
  internal class EnergyConsumptionFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public EnergyConsumptionFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (EnergyConsumption);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new EnergyConsumptionViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateEnergyConsumption(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreateEnergyConsumption("Energieverbrauch", "Stellt den Verbrauch von Energie dar.", "Energieverbrauch", "CfSite.png", DateTime.Now, DateTime.Now.AddYears(1), 2000, EnergySource.Ecostrom),
          responsibleSubjects);
    }

    private EnergyConsumption CreateEnergyConsumption(string name, string description, string tag, string iconId, DateTime start, DateTime finish, double consumption = 0,
      EnergySource energySource = EnergySource.Strommix, double carbonProduction = 0)
    {
      return new EnergyConsumption
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Consumption = consumption,
               m_EnergySource = (int) energySource,
               CarbonProduction = carbonProduction
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Maschinenverbrauch", "Einen neuen Energieverbrauch pro Maschinen(-zustand) erstellen.", "../CarbonFootprints/Resources/Images/energyconsumption.png")]
  internal class MachineEnergyConsumptionFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public MachineEnergyConsumptionFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (MachineEnergyConsumption);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new MachineEnergyConsumptionViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateMachineEnergyConsumption(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreateMachineEnergyConsumption("Maschinenverbrauch", "Stellt den Energieverbrauch von Maschinen(-zuständen) dar.", "Energieverbrauch", "CfSite.png", DateTime.Now, DateTime.Now.AddYears(1),
            EnergySource.Ecostrom, 1000, 100, 2000, 250), responsibleSubjects);
    }

    private MachineEnergyConsumption CreateMachineEnergyConsumption(string name, string description, string tag, string iconId, DateTime start, DateTime finish,
      EnergySource energySource = EnergySource.Strommix, double hourInStandbyState = 0, double consumptionPerHourForStandby = 0, double hourInProcessingState = 0,
      double consumptionPerHourForProcessing = 0, double carbonProduction = 0)
    {
      return new MachineEnergyConsumption
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_EnergySource = (int) energySource,
               HoursInProcessingState = hourInProcessingState,
               HoursInStandbyState = hourInStandbyState,
               ConsumptionPerHourForStandby = consumptionPerHourForStandby,
               ConsumptionPerHourForProcessing = consumptionPerHourForProcessing,
               CarbonProduction = carbonProduction
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Öffentliches Verkehrsmittel", "Einen neues öffentliches Verkehrsmittel erstellen.", "../CarbonFootprints/Resources/Images/publictransport.png")]
  internal class PublicTransportFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public PublicTransportFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (PublicTransport);
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new PublicTransportViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(string name, string description, string tag, string iconId, DateTime start, DateTime finish, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreatePublicTransport(name, description, tag, iconId, start, finish), responsibleSubjects);
    }

    public PositionViewModel CreatePredefinedPosition(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreatePublicTransport("Öffentlicher Verkehr", "Stellt den öffentlichen Verkehr bereit.", "Öffentlicher Verkehr", "CfPublicTransport.png", DateTime.Now, DateTime.Now.AddYears(1), 300100,
            PublicTransportViewModel.TransportTypeEnum.Reisebus), responsibleSubjects);
    }

    private PublicTransport CreatePublicTransport(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0,
      PublicTransportViewModel.TransportTypeEnum transportType = PublicTransportViewModel.TransportTypeEnum.Linienbus)
    {
      return new PublicTransport
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_TransportType = (int) transportType,
               Kilometrage = kilometrage
             };
    }
  }
}