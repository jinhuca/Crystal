using Crystal.Telemetry.Hardware;

namespace Crystal.Information.TypeDefinitions;

public class UpdateVisitor : IVisitor {
  public void VisitComputer(IComputer computer) => computer.Traverse(this);

  public void VisitHardware(IHardware hardware) {
    if(hardware == null) return;
    hardware.Update();
    foreach (IHardware subHardware in hardware.SubHardware)
      subHardware.Accept(this);
  }

  public void VisitSensor(ISensor sensor) { }

  public void VisitParameter(IParameter parameter) { }
}
