using Crystal.Provider.Telemetry.Hardware;
using System;
using System.Collections.Generic;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class SensorVisitorTests {
  [Fact]
  public void Ctor_NullHandler_Throws() {
    Assert.Throws<ArgumentNullException>(() => new SensorVisitor(null));
  }

  [Fact]
  public void VisitComputer_Null_Throws() {
    var visitor = new SensorVisitor(_ => { });
    Assert.Throws<ArgumentNullException>(() => visitor.VisitComputer(null));
  }

  [Fact]
  public void VisitHardware_Null_Throws() {
    var visitor = new SensorVisitor(_ => { });
    Assert.Throws<ArgumentNullException>(() => visitor.VisitHardware(null));
  }

  [Fact]
  public void VisitSensor_InvokesHandlerWithSensor() {
    ISensor? received = null;
    var visitor = new SensorVisitor(s => received = s);
    var sensor = new MockSensor(new Identifier("cpu", "0"));

    visitor.VisitSensor(sensor);

    Assert.Same(sensor, received);
  }

  [Fact]
  public void VisitComputer_TraversesDownToEverySensor() {
    var visited = new List<ISensor>();
    var visitor = new SensorVisitor(s => visited.Add(s));

    var sensorA = new MockSensor(new Identifier("cpu", "0"));
    var sensorB = new MockSensor(new Identifier("cpu", "1"));
    var hardware = new MockHardware { Sensors = new ISensor[] { sensorA, sensorB } };
    var computer = new MockComputer { Hardware = new List<IHardware> { hardware } };

    visitor.VisitComputer(computer);

    Assert.True(computer.Traversed);
    Assert.True(hardware.Traversed);
    Assert.Equal(new ISensor[] { sensorA, sensorB }, visited);
  }

  [Fact]
  public void VisitHardware_TraversesItsSensors() {
    var visited = new List<ISensor>();
    var visitor = new SensorVisitor(s => visited.Add(s));

    var sensor = new MockSensor(new Identifier("gpu", "0"));
    var hardware = new MockHardware { Sensors = new ISensor[] { sensor } };

    visitor.VisitHardware(hardware);

    Assert.True(hardware.Traversed);
    Assert.Single(visited, sensor);
  }
}
