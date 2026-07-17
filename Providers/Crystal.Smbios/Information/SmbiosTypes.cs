// Compatibility: bring the nested types namespace into global usage for
// existing code that expects enums in Crystal.Smbios.
global using CrystalMonitorLib.Smbios.Structures;

// Intentionally empty file. The global using above makes types from
// Crystal.Smbios.Types available without changing existing code.
