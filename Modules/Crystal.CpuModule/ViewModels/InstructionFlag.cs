namespace Crystal.CpuModule.ViewModels;

/// <summary>One entry in the "Instruction Set Features Available" grid: the ISA
/// mnemonic and whether this CPU reports it.</summary>
public sealed record InstructionFlag(string Name, bool IsAvailable);
