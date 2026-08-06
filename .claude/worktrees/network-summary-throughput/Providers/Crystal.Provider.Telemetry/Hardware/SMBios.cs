using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Win32.System.SystemInformation;

namespace Crystal.Provider.Telemetry.Hardware;

/// <summary>
/// System enclosure security status based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.4.3</see>.
/// </summary>
public enum SystemEnclosureSecurityStatus {
  /// <summary>The security status is of an other type.</summary>
  Other = 1,

  /// <summary>The security status is unknown.</summary>
  Unknown,

  /// <summary>No physical security status is present.</summary>
  None,

  /// <summary>The external interface is locked out.</summary>
  ExternalInterfaceLockedOut,

  /// <summary>The external interface is enabled.</summary>
  ExternalInterfaceEnabled
}

/// <summary>
/// System enclosure state based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.4.2</see>.
/// </summary>
public enum SystemEnclosureState {
  /// <summary>The state is of an other type.</summary>
  Other = 1,

  /// <summary>The state is unknown.</summary>
  Unknown,

  /// <summary>The enclosure is in a safe state.</summary>
  Safe,

  /// <summary>The enclosure is in a warning state.</summary>
  Warning,

  /// <summary>The enclosure is in a critical state.</summary>
  Critical,

  /// <summary>The enclosure is in a non-recoverable state.</summary>
  NonRecoverable
}

/// <summary>
/// System enclosure type based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.4.1</see>.
/// </summary>
public enum SystemEnclosureType {
  /// <summary>An other enclosure type.</summary>
  Other = 1,

  /// <summary>An unknown enclosure type.</summary>
  Unknown,

  /// <summary>A desktop enclosure.</summary>
  Desktop,

  /// <summary>A low-profile desktop enclosure.</summary>
  LowProfileDesktop,

  /// <summary>A pizza box enclosure.</summary>
  PizzaBox,

  /// <summary>A mini tower enclosure.</summary>
  MiniTower,

  /// <summary>A tower enclosure.</summary>
  Tower,

  /// <summary>A portable enclosure.</summary>
  Portable,

  /// <summary>A laptop enclosure.</summary>
  Laptop,

  /// <summary>A notebook enclosure.</summary>
  Notebook,

  /// <summary>A hand-held enclosure.</summary>
  HandHeld,

  /// <summary>A docking station enclosure.</summary>
  DockingStation,

  /// <summary>An all-in-one enclosure.</summary>
  AllInOne,

  /// <summary>A sub-notebook enclosure.</summary>
  SubNotebook,

  /// <summary>A space-saving enclosure.</summary>
  SpaceSaving,

  /// <summary>A lunch box enclosure.</summary>
  LunchBox,

  /// <summary>A main server chassis enclosure.</summary>
  MainServerChassis,

  /// <summary>An expansion chassis enclosure.</summary>
  ExpansionChassis,

  /// <summary>A sub-chassis enclosure.</summary>
  SubChassis,

  /// <summary>A bus expansion chassis enclosure.</summary>
  BusExpansionChassis,

  /// <summary>A peripheral chassis enclosure.</summary>
  PeripheralChassis,

  /// <summary>A RAID chassis enclosure.</summary>
  RaidChassis,

  /// <summary>A rack-mount chassis enclosure.</summary>
  RackMountChassis,

  /// <summary>A sealed-case PC enclosure.</summary>
  SealedCasePc,

  /// <summary>A multi-system chassis enclosure.</summary>
  MultiSystemChassis,

  /// <summary>A CompactPCI enclosure.</summary>
  CompactPci,

  /// <summary>An AdvancedTCA enclosure.</summary>
  AdvancedTca,

  /// <summary>A blade enclosure.</summary>
  Blade,

  /// <summary>A blade enclosure chassis.</summary>
  BladeEnclosure,

  /// <summary>A tablet enclosure.</summary>
  Tablet,

  /// <summary>A convertible enclosure.</summary>
  Convertible,

  /// <summary>A detachable enclosure.</summary>
  Detachable,

  /// <summary>An IoT gateway enclosure.</summary>
  IoTGateway,

  /// <summary>An embedded PC enclosure.</summary>
  EmbeddedPc,

  /// <summary>A mini PC enclosure.</summary>
  MiniPc,

  /// <summary>A stick PC enclosure.</summary>
  StickPc
}

/// <summary>
/// Processor family based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.5.2</see>.
/// </summary>
public enum ProcessorFamily {
  /// <summary>An other processor family.</summary>
  Other = 1,

  /// <summary>Intel 8086 processor.</summary>
  Intel8086 = 3,

  /// <summary>Intel 80286 processor.</summary>
  Intel80286 = 4,

  /// <summary>Intel 386 processor.</summary>
  Intel386,

  /// <summary>Intel 486 processor.</summary>
  Intel486,

  /// <summary>Intel 8087 math coprocessor.</summary>
  Intel8087,

  /// <summary>Intel 80287 math coprocessor.</summary>
  Intel80287,

  /// <summary>Intel 80387 math coprocessor.</summary>
  Intel80387,

  /// <summary>Intel 80487 math coprocessor.</summary>
  Intel80487,

  /// <summary>Intel Pentium processor.</summary>
  IntelPentium,

  /// <summary>Intel Pentium Pro processor.</summary>
  IntelPentiumPro,

  /// <summary>Intel Pentium II processor.</summary>
  IntelPentiumII,

  /// <summary>Intel Pentium with MMX technology processor.</summary>
  IntelPentiumMMX,

  /// <summary>Intel Celeron processor.</summary>
  IntelCeleron,

  /// <summary>Intel Pentium II Xeon processor.</summary>
  IntelPentiumIIXeon,

  /// <summary>Intel Pentium III processor.</summary>
  IntelPentiumIII,

  /// <summary>M1 family processor.</summary>
  M1,

  /// <summary>M2 family processor.</summary>
  M2,

  /// <summary>Intel Celeron M processor.</summary>
  IntelCeleronM,

  /// <summary>Intel Pentium 4 HT processor.</summary>
  IntelPentium4HT,

  /// <summary>AMD Duron processor.</summary>
  AmdDuron = 24,

  /// <summary>AMD K5 family processor.</summary>
  AmdK5,

  /// <summary>AMD K6 family processor.</summary>
  AmdK6,

  /// <summary>AMD K6-2 processor.</summary>
  AmdK62,

  /// <summary>AMD K6-3 processor.</summary>
  AmdK63,

  /// <summary>AMD Athlon processor.</summary>
  AmdAthlon,

  /// <summary>AMD 29000 family processor.</summary>
  Amd2900,

  /// <summary>AMD K6-2+ processor.</summary>
  AmdK62Plus,

  /// <summary>PowerPC family processor.</summary>
  PowerPc,

  /// <summary>PowerPC 601 processor.</summary>
  PowerPc601,

  /// <summary>PowerPC 603 processor.</summary>
  PowerPc603,

  /// <summary>PowerPC 603+ processor.</summary>
  PowerPc603Plus,

  /// <summary>PowerPC 604 processor.</summary>
  PowerPc604,

  /// <summary>PowerPC 620 processor.</summary>
  PowerPc620,

  /// <summary>PowerPC x704 processor.</summary>
  PowerPcx704,

  /// <summary>PowerPC 750 processor.</summary>
  PowerPc750,

  /// <summary>Intel Core Duo processor.</summary>
  IntelCoreDuo,

  /// <summary>Intel Core Duo mobile processor.</summary>
  IntelCoreDuoMobile,

  /// <summary>Intel Core Solo mobile processor.</summary>
  IntelCoreSoloMobile,

  /// <summary>Intel Atom processor.</summary>
  IntelAtom,

  /// <summary>Intel Core M processor.</summary>
  IntelCoreM,

  /// <summary>Intel Core m3 processor.</summary>
  IntelCoreM3,

  /// <summary>Intel Core m5 processor.</summary>
  IntelCoreM5,

  /// <summary>Intel Core m7 processor.</summary>
  IntelCoreM7,

  /// <summary>DEC Alpha family processor.</summary>
  Alpha,

  /// <summary>DEC Alpha 21064 processor.</summary>
  Alpha21064,

  /// <summary>DEC Alpha 21066 processor.</summary>
  Alpha21066,

  /// <summary>DEC Alpha 21164 processor.</summary>
  Alpha21164,

  /// <summary>DEC Alpha 21164PC processor.</summary>
  Alpha21164Pc,

  /// <summary>DEC Alpha 21164a processor.</summary>
  Alpha21164a,

  /// <summary>DEC Alpha 21264 processor.</summary>
  Alpha21264,

  /// <summary>DEC Alpha 21364 processor.</summary>
  Alpha21364,

  /// <summary>AMD Turion II Ultra Dual-Core Mobile M processor.</summary>
  AmdTurionIIUltraDualCoreMobileM,

  /// <summary>AMD Turion II Dual-Core Mobile M processor.</summary>
  AmdTurionDualCoreMobileM,

  /// <summary>AMD Athlon II Dual-Core M processor.</summary>
  AmdAthlonIIDualCoreM,

  /// <summary>AMD Opteron 6100 series processor.</summary>
  AmdOpteron6100Series,

  /// <summary>AMD Opteron 4100 series processor.</summary>
  AmdOpteron4100Series,

  /// <summary>AMD Opteron 6200 series processor.</summary>
  AmdOpteron6200Series,

  /// <summary>AMD Opteron 4200 series processor.</summary>
  AmdOpteron4200Series,

  /// <summary>AMD FX series processor.</summary>
  AmdFxSeries,

  /// <summary>MIPS family processor.</summary>
  Mips,

  /// <summary>MIPS R4000 processor.</summary>
  MipsR4000,

  /// <summary>MIPS R4200 processor.</summary>
  MipsR4200,

  /// <summary>MIPS R4400 processor.</summary>
  MipsR4400,

  /// <summary>MIPS R4600 processor.</summary>
  MipsR4600,

  /// <summary>MIPS R10000 processor.</summary>
  MipsR10000,

  /// <summary>AMD C-Series processor.</summary>
  AmdCSeries,

  /// <summary>AMD E-Series processor.</summary>
  AmdESeries,

  /// <summary>AMD A-Series processor.</summary>
  AmdASeries,

  /// <summary>AMD G-Series processor.</summary>
  AmdGSeries,

  /// <summary>AMD Z-Series processor.</summary>
  AmdZSeries,

  /// <summary>AMD R-Series processor.</summary>
  AmdRSeries,

  /// <summary>AMD Opteron 4300 series processor.</summary>
  AmdOpteron4300Series,

  /// <summary>AMD Opteron 6300 series processor.</summary>
  AmdOpteron6300Series,

  /// <summary>AMD Opteron 3300 series processor.</summary>
  AmdOpteron3300Series,

  /// <summary>AMD FirePro series processor.</summary>
  AmdFireProSeries,

  /// <summary>SPARC family processor.</summary>
  Sparc,

  /// <summary>SuperSPARC processor.</summary>
  SuperSparc,

  /// <summary>microSPARC II processor.</summary>
  MicroSparcII,

  /// <summary>microSPARC IIep processor.</summary>
  MicroSparcIIep,

  /// <summary>UltraSPARC processor.</summary>
  UltraSparc,

  /// <summary>UltraSPARC II processor.</summary>
  UltraSparcII,

  /// <summary>UltraSPARC IIi processor.</summary>
  UltraSparcIIi,

  /// <summary>UltraSPARC III processor.</summary>
  UltraSparcIII,

  /// <summary>UltraSPARC IIIi processor.</summary>
  UltraSparcIIIi,

  /// <summary>Motorola 68040 processor.</summary>
  Motorola68040 = 96,

  /// <summary>Motorola 68xxx family processor.</summary>
  Motorola68xxx,

  /// <summary>Motorola 68000 processor.</summary>
  Motorola68000,

  /// <summary>Motorola 68010 processor.</summary>
  Motorola68010,

  /// <summary>Motorola 68020 processor.</summary>
  Motorola68020,

  /// <summary>Motorola 68030 processor.</summary>
  Motorola68030,

  /// <summary>AMD Athlon X4 Quad-Core processor.</summary>
  AmdAthlonX4QuadCore,

  /// <summary>AMD Opteron X1000 series processor.</summary>
  AmdOpteronX1000Series,

  /// <summary>AMD Opteron X2000 series processor.</summary>
  AmdOpteronX2000Series,

  /// <summary>AMD Opteron A-Series processor.</summary>
  AmdOpteronASeries,

  /// <summary>AMD Opteron X3000 series processor.</summary>
  AmdOpteronX3000Series,

  /// <summary>AMD Zen family processor.</summary>
  AmdZen,

  /// <summary>Hobbit family processor.</summary>
  Hobbit = 112,

  /// <summary>Transmeta Crusoe TM5000 family processor.</summary>
  CrusoeTm5000 = 120,

  /// <summary>Transmeta Crusoe TM3000 family processor.</summary>
  CrusoeTm3000,

  /// <summary>Transmeta Efficeon TM8000 family processor.</summary>
  EfficeonTm8000,

  /// <summary>Weitek processor.</summary>
  Weitek = 128,

  /// <summary>Intel Itanium processor.</summary>
  IntelItanium = 130,

  /// <summary>AMD Athlon 64 processor.</summary>
  AmdAthlon64,

  /// <summary>AMD Opteron processor.</summary>
  AmdOpteron,

  /// <summary>AMD Sempron processor.</summary>
  AmdSempron,

  /// <summary>AMD Turion 64 mobile processor.</summary>
  AmdTurio64Mobile,

  /// <summary>AMD Opteron Dual-Core processor.</summary>
  AmdOpteronDualCore,

  /// <summary>AMD Athlon 64 X2 Dual-Core processor.</summary>
  AmdAthlon64X2DualCore,

  /// <summary>AMD Turion 64 X2 mobile processor.</summary>
  AmdTurion64X2Mobile,

  /// <summary>AMD Opteron Quad-Core processor.</summary>
  AmdOpteronQuadCore,

  /// <summary>AMD Opteron third-generation processor.</summary>
  AmdOpteronThirdGen,

  /// <summary>AMD Phenom FX Quad-Core processor.</summary>
  AmdPhenomFXQuadCore,

  /// <summary>AMD Phenom X4 Quad-Core processor.</summary>
  AmdPhenomX4QuadCore,

  /// <summary>AMD Phenom X2 Dual-Core processor.</summary>
  AmdPhenomX2DualCore,

  /// <summary>AMD Athlon X2 Dual-Core processor.</summary>
  AmdAthlonX2DualCore,

  /// <summary>PA-RISC family processor.</summary>
  PaRisc,

  /// <summary>PA-RISC 8500 processor.</summary>
  PaRisc8500,

  /// <summary>PA-RISC 8000 processor.</summary>
  PaRisc8000,

  /// <summary>PA-RISC 7300LC processor.</summary>
  PaRisc7300LC,

  /// <summary>PA-RISC 7200 processor.</summary>
  PaRisc7200,

  /// <summary>PA-RISC 7100LC processor.</summary>
  PaRisc7100LC,

  /// <summary>PA-RISC 7100 processor.</summary>
  PaRisc7100,

  /// <summary>V30 family processor.</summary>
  V30 = 160,

  /// <summary>Intel Xeon 3200 Quad-Core series processor.</summary>
  IntelXeon3200QuadCoreSeries,

  /// <summary>Intel Xeon 3000 Dual-Core series processor.</summary>
  IntelXeon3000DualCoreSeries,

  /// <summary>Intel Xeon 5300 Quad-Core series processor.</summary>
  IntelXeon5300QuadCoreSeries,

  /// <summary>Intel Xeon 5100 Dual-Core series processor.</summary>
  IntelXeon5100DualCoreSeries,

  /// <summary>Intel Xeon 5000 Dual-Core series processor.</summary>
  IntelXeon5000DualCoreSeries,

  /// <summary>Intel Xeon LV Dual-Core processor.</summary>
  IntelXeonLVDualCore,

  /// <summary>Intel Xeon ULV Dual-Core processor.</summary>
  IntelXeonULVDualCore,

  /// <summary>Intel Xeon 7100 series processor.</summary>
  IntelXeon7100Series,

  /// <summary>Intel Xeon 5400 series processor.</summary>
  IntelXeon5400Series,

  /// <summary>Intel Xeon Quad-Core processor.</summary>
  IntelXeonQuadCore,

  /// <summary>Intel Xeon 5200 Dual-Core series processor.</summary>
  IntelXeon5200DualCoreSeries,

  /// <summary>Intel Xeon 7200 Dual-Core series processor.</summary>
  IntelXeon7200DualCoreSeries,

  /// <summary>Intel Xeon 7300 Quad-Core series processor.</summary>
  IntelXeon7300QuadCoreSeries,

  /// <summary>Intel Xeon 7400 Quad-Core series processor.</summary>
  IntelXeon7400QuadCoreSeries,

  /// <summary>Intel Xeon 7400 Multi-Core series processor.</summary>
  IntelXeon7400MultiCoreSeries,

  /// <summary>Intel Pentium III Xeon processor.</summary>
  IntelPentiumIIIXeon,

  /// <summary>Intel Pentium III with SpeedStep technology processor.</summary>
  IntelPentiumIIISpeedStep,

  /// <summary>Intel Pentium 4 processor.</summary>
  IntelPentium4,

  /// <summary>Intel Xeon processor.</summary>
  IntelXeon,

  /// <summary>IBM AS/400 family processor.</summary>
  As400,

  /// <summary>Intel Xeon MP processor.</summary>
  IntelXeonMP,

  /// <summary>AMD Athlon XP processor.</summary>
  AmdAthlonXP,

  /// <summary>AMD Athlon MP processor.</summary>
  AmdAthlonMP,

  /// <summary>Intel Itanium 2 processor.</summary>
  IntelItanium2,

  /// <summary>Intel Pentium M processor.</summary>
  IntelPentiumM,

  /// <summary>Intel Celeron D processor.</summary>
  IntelCeleronD,

  /// <summary>Intel Pentium D processor.</summary>
  IntelPentiumD,

  /// <summary>Intel Pentium Extreme Edition processor.</summary>
  IntelPentiumExtreme,

  /// <summary>Intel Core Solo processor.</summary>
  IntelCoreSolo,

  /// <summary>Intel Core 2 Duo processor.</summary>
  IntelCore2Duo = 191,

  /// <summary>Intel Core 2 Solo processor.</summary>
  IntelCore2Solo,

  /// <summary>Intel Core 2 Extreme processor.</summary>
  IntelCore2Extreme,

  /// <summary>Intel Core 2 Quad processor.</summary>
  IntelCore2Quad,

  /// <summary>Intel Core 2 Extreme mobile processor.</summary>
  IntelCore2ExtremeMobile,

  /// <summary>Intel Core 2 Duo mobile processor.</summary>
  IntelCore2DuoMobile,

  /// <summary>Intel Core 2 Solo mobile processor.</summary>
  IntelCore2SoloMobile,

  /// <summary>Intel Core i7 processor.</summary>
  IntelCoreI7,

  /// <summary>Intel Celeron Dual-Core processor.</summary>
  IntelCeleronDualCore,

  /// <summary>IBM 390 family processor.</summary>
  Ibm390,

  /// <summary>PowerPC G4 processor.</summary>
  PowerPcG4,

  /// <summary>PowerPC G5 processor.</summary>
  PowerPcG5,

  /// <summary>ESA/390 G6 processor.</summary>
  Esa390G6,

  /// <summary>z/Architecture processor.</summary>
  ZArchitecture,

  /// <summary>Intel Core i5 processor.</summary>
  IntelCoreI5,

  /// <summary>Intel Core i3 processor.</summary>
  IntelCoreI3,

  /// <summary>Intel Core i9 processor.</summary>
  IntelCoreI9,

  /// <summary>VIA C7-M processor.</summary>
  ViaC7M = 210,

  /// <summary>VIA C7-D processor.</summary>
  ViaC7D,

  /// <summary>VIA C7 processor.</summary>
  ViaC7,

  /// <summary>VIA Eden processor.</summary>
  ViaEden,

  /// <summary>Intel Xeon Multi-Core processor.</summary>
  IntelXeonMultiCore,

  /// <summary>Intel Xeon 3xxx Dual-Core series processor.</summary>
  IntelXeon3xxxDualCoreSeries,

  /// <summary>Intel Xeon 3xxx Quad-Core series processor.</summary>
  IntelXeon3xxxQuadCoreSeries,

  /// <summary>VIA Nano processor.</summary>
  ViaNano,

  /// <summary>Intel Xeon 5xxx Dual-Core series processor.</summary>
  IntelXeon5xxxDualCoreSeries,

  /// <summary>Intel Xeon 5xxx Quad-Core series processor.</summary>
  IntelXeon5xxxQuadCoreSeries,

  /// <summary>Intel Xeon 7xxx Dual-Core series processor.</summary>
  IntelXeon7xxxDualCoreSeries = 221,

  /// <summary>Intel Xeon 7xxx Quad-Core series processor.</summary>
  IntelXeon7xxxQuadCoreSeries,

  /// <summary>Intel Xeon 7xxx Multi-Core series processor.</summary>
  IntelXeon7xxxMultiCoreSeries,

  /// <summary>Intel Xeon 3400 Multi-Core series processor.</summary>
  IntelXeon3400MultiCoreSeries,

  /// <summary>AMD Opteron 3000 series processor.</summary>
  AmdOpteron3000Series = 228,

  /// <summary>AMD Sempron II processor.</summary>
  AmdSempronII,

  /// <summary>AMD Opteron Quad-Core embedded processor.</summary>
  AmdOpteronQuadCoreEmbedded,

  /// <summary>AMD Phenom Triple-Core processor.</summary>
  AmdPhenomTripleCore,

  /// <summary>AMD Turion Ultra Dual-Core mobile processor.</summary>
  AmdTurionUltraDualCoreMobile,

  /// <summary>AMD Turion Dual-Core mobile processor.</summary>
  AmdTurionDualCoreMobile,

  /// <summary>AMD Turion Dual-Core processor.</summary>
  AmdTurionDualCore,

  /// <summary>AMD Athlon Dual-Core processor.</summary>
  AmdAthlonDualCore,

  /// <summary>AMD Sempron SI processor.</summary>
  AmdSempronSI,

  /// <summary>AMD Phenom II processor.</summary>
  AmdPhenomII,

  /// <summary>AMD Athlon II processor.</summary>
  AmdAthlonII,

  /// <summary>AMD Opteron Six-Core processor.</summary>
  AmdOpteronSixCore,

  /// <summary>AMD Sempron M processor.</summary>
  AmdSempronM,

  /// <summary>Intel i860 processor.</summary>
  IntelI860 = 250,

  /// <summary>Intel i960 processor.</summary>
  IntelI960,

  /// <summary>ARMv7 architecture processor.</summary>
  ArmV7 = 256,

  /// <summary>ARMv8 architecture processor.</summary>
  ArmV8,

  /// <summary>Hitachi SH-3 processor.</summary>
  HitachiSh3,

  /// <summary>Hitachi SH-4 processor.</summary>
  HitachiSh4,

  /// <summary>ARM family processor.</summary>
  Arm,

  /// <summary>StrongARM processor.</summary>
  StrongArm,

  /// <summary>6x86 processor.</summary>
  _686,

  /// <summary>MediaGX processor.</summary>
  MediaGX,

  /// <summary>MII processor.</summary>
  MII,

  /// <summary>WinChip processor.</summary>
  WinChip,

  /// <summary>DSP (digital signal processor).</summary>
  Dsp,

  /// <summary>Video processor.</summary>
  VideoProcessor
}

/// <summary>
/// Processor characteristics based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.5.9</see>.
/// </summary>
[Flags]
public enum ProcessorCharacteristics {
  /// <summary>No characteristics are defined.</summary>
  None = 0,

  /// <summary>The processor is 64-bit capable.</summary>
  _64BitCapable = 1,

  /// <summary>The processor is multi-core.</summary>
  MultiCore = 2,

  /// <summary>The processor supports multiple hardware threads.</summary>
  HardwareThread = 4,

  /// <summary>The processor supports execute protection.</summary>
  ExecuteProtection = 8,

  /// <summary>The processor supports enhanced virtualization.</summary>
  EnhancedVirtualization = 16,

  /// <summary>The processor supports power/performance control.</summary>
  PowerPerformanceControl = 32,

  /// <summary>The processor is 128-bit capable.</summary>
  _128BitCapable = 64
}

/// <summary>
/// Processor type based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.5.1</see>.
/// </summary>
public enum ProcessorType {
  /// <summary>An other processor type.</summary>
  Other = 1,

  /// <summary>An unknown processor type.</summary>
  Unknown,

  /// <summary>A central processor (CPU).</summary>
  CentralProcessor,

  /// <summary>A math processor.</summary>
  MathProcessor,

  /// <summary>A DSP (digital signal) processor.</summary>
  DspProcessor,

  /// <summary>A video processor.</summary>
  VideoProcessor
}

/// <summary>
/// Processor socket based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.5.5</see>.
/// </summary>
public enum ProcessorSocket {
  /// <summary>An other socket type.</summary>
  Other = 1,

  /// <summary>An unknown socket type.</summary>
  Unknown,

  /// <summary>A daughter board socket.</summary>
  DaughterBoard,

  /// <summary>A ZIF (zero insertion force) socket.</summary>
  ZifSocket,

  /// <summary>A replaceable piggy-back socket.</summary>
  PiggyBack,

  /// <summary>No socket.</summary>
  None,

  /// <summary>A LIF (low insertion force) socket.</summary>
  LifSocket,

  /// <summary>Socket ZIF 423.</summary>
  Zif423 = 13,

  /// <summary>Socket A (Socket 462).</summary>
  A,

  /// <summary>Socket ZIF 478.</summary>
  Zif478,

  /// <summary>Socket ZIF 754.</summary>
  Zif754,

  /// <summary>Socket ZIF 940.</summary>
  Zif940,

  /// <summary>Socket ZIF 939.</summary>
  Zif939,

  /// <summary>Socket mPGA604.</summary>
  MPga604,

  /// <summary>Socket LGA771.</summary>
  Lga771,

  /// <summary>Socket LGA775.</summary>
  Lga775,

  /// <summary>Socket S1.</summary>
  S1,

  /// <summary>Socket AM2.</summary>
  AM2,

  /// <summary>Socket F (1207).</summary>
  F,

  /// <summary>Socket LGA1366.</summary>
  Lga1366,

  /// <summary>Socket G34.</summary>
  G34,

  /// <summary>Socket AM3.</summary>
  AM3,

  /// <summary>Socket C32.</summary>
  C32,

  /// <summary>Socket LGA1156.</summary>
  Lga1156,

  /// <summary>Socket LGA1567.</summary>
  Lga1567,

  /// <summary>Socket PGA988A.</summary>
  Pga988A,

  /// <summary>Socket BGA1288.</summary>
  Bga1288,

  /// <summary>Socket rPGA988B.</summary>
  RPga088B,

  /// <summary>Socket BGA1023.</summary>
  Bga1023,

  /// <summary>Socket BGA1224.</summary>
  Bga1224,

  /// <summary>Socket LGA1155.</summary>
  Lga1155,

  /// <summary>Socket LGA1356.</summary>
  Lga1356,

  /// <summary>Socket LGA2011.</summary>
  Lga2011,

  /// <summary>Socket FS1.</summary>
  FS1,

  /// <summary>Socket FS2.</summary>
  FS2,

  /// <summary>Socket FM1.</summary>
  FM1,

  /// <summary>Socket FM2.</summary>
  FM2,

  /// <summary>Socket LGA2011-3.</summary>
  Lga20113,

  /// <summary>Socket LGA1356-3.</summary>
  Lga13563,

  /// <summary>Socket LGA1150.</summary>
  Lga1150,

  /// <summary>Socket BGA1168.</summary>
  Bga1168,

  /// <summary>Socket BGA1234.</summary>
  Bga1234,

  /// <summary>Socket BGA1364.</summary>
  Bga1364,

  /// <summary>Socket AM4.</summary>
  AM4,

  /// <summary>Socket LGA1151.</summary>
  Lga1151,

  /// <summary>Socket BGA1356.</summary>
  Bga1356,

  /// <summary>Socket BGA1440.</summary>
  Bga1440,

  /// <summary>Socket BGA1515.</summary>
  Bga1515,

  /// <summary>Socket LGA3647-1.</summary>
  Lga36471,

  /// <summary>Socket SP3.</summary>
  SP3,

  /// <summary>Socket SP3r2.</summary>
  SP3R2,

  /// <summary>Socket LGA2066.</summary>
  Lga2066,

  /// <summary>Socket BGA1510.</summary>
  Bga1510,

  /// <summary>Socket BGA1528.</summary>
  Bga1528,

  /// <summary>Socket LGA4189.</summary>
  Lga4189
}

/// <summary>
/// System wake-up type based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.2.2</see>.
/// </summary>
public enum SystemWakeUp {
  /// <summary>A reserved wake-up type.</summary>
  Reserved,

  /// <summary>An other wake-up type.</summary>
  Other,

  /// <summary>An unknown wake-up type.</summary>
  Unknown,

  /// <summary>The system was woken by an APM timer.</summary>
  ApmTimer,

  /// <summary>The system was woken by a modem ring.</summary>
  ModemRing,

  /// <summary>The system was woken by a LAN remote request.</summary>
  LanRemote,

  /// <summary>The system was woken by the power switch.</summary>
  PowerSwitch,

  /// <summary>The system was woken by a PCI PME# signal.</summary>
  PciPme,

  /// <summary>The system was woken because AC power was restored.</summary>
  AcPowerRestored
}

/// <summary>
/// Cache associativity based on <see href="https://www.dmtf.org/dsp/DSP0134">DMTF SMBIOS Reference Specification v.3.3.0, Chapter 7.8.5</see>.
/// </summary>
public enum CacheAssociativity {
  /// <summary>An other associativity type.</summary>
  Other = 1,

  /// <summary>An unknown associativity type.</summary>
  Unknown,

  /// <summary>Direct-mapped cache.</summary>
  DirectMapped,

  /// <summary>2-way set-associative cache.</summary>
  _2Way,

  /// <summary>4-way set-associative cache.</summary>
  _4Way,

  /// <summary>Fully associative cache.</summary>
  FullyAssociative,

  /// <summary>8-way set-associative cache.</summary>
  _8Way,

  /// <summary>16-way set-associative cache.</summary>
  _16Way,

  /// <summary>12-way set-associative cache.</summary>
  _12Way,

  /// <summary>24-way set-associative cache.</summary>
  _24Way,

  /// <summary>32-way set-associative cache.</summary>
  _32Way,

  /// <summary>48-way set-associative cache.</summary>
  _48Way,

  /// <summary>64-way set-associative cache.</summary>
  _64Way,

  /// <summary>20-way set-associative cache.</summary>
  _20Way
}

/// <summary>
/// Processor cache level.
/// </summary>
public enum CacheDesignation {
  /// <summary>An other or unspecified cache level.</summary>
  Other,

  /// <summary>Level 1 (L1) cache.</summary>
  L1,

  /// <summary>Level 2 (L2) cache.</summary>
  L2,

  /// <summary>Level 3 (L3) cache.</summary>
  L3
}

/// <summary>
/// Memory type.
/// </summary>
public enum MemoryType {
  /// <summary>An other memory type.</summary>
  Other = 0x01,

  /// <summary>An unknown memory type.</summary>
  Unknown = 0x02,

  /// <summary>DRAM (Dynamic Random-Access Memory).</summary>
  DRAM = 0x03,

  /// <summary>EDRAM (Enhanced Dynamic Random-Access Memory).</summary>
  EDRAM = 0x04,

  /// <summary>VRAM (Video Random-Access Memory).</summary>
  VRAM = 0x05,

  /// <summary>SRAM (Static Random-Access Memory).</summary>
  SRAM = 0x06,

  /// <summary>RAM (Random-Access Memory).</summary>
  RAM = 0x07,

  /// <summary>ROM (Read-Only Memory).</summary>
  ROM = 0x08,

  /// <summary>Flash memory.</summary>
  FLASH = 0x09,

  /// <summary>EEPROM (Electrically Erasable Programmable Read-Only Memory).</summary>
  EEPROM = 0x0a,

  /// <summary>FEPROM (Flash Erasable Programmable Read-Only Memory).</summary>
  FEPROM = 0x0b,

  /// <summary>EPROM (Erasable Programmable Read-Only Memory).</summary>
  EPROM = 0x0c,

  /// <summary>CDRAM (Cached Dynamic Random-Access Memory).</summary>
  CDRAM = 0x0d,

  /// <summary>3DRAM (3D Random-Access Memory).</summary>
  _3DRAM = 0x0e,

  /// <summary>SDRAM (Synchronous Dynamic Random-Access Memory).</summary>
  SDRAM = 0x0f,

  /// <summary>SGRAM (Synchronous Graphics Random-Access Memory).</summary>
  SGRAM = 0x10,

  /// <summary>RDRAM (Rambus Dynamic Random-Access Memory).</summary>
  RDRAM = 0x11,

  /// <summary>DDR (Double Data Rate) SDRAM.</summary>
  DDR = 0x12,

  /// <summary>DDR2 SDRAM.</summary>
  DDR2 = 0x13,

  /// <summary>DDR2 FB-DIMM (Fully Buffered DIMM).</summary>
  DDR2_FBDIMM = 0x14,

  /// <summary>DDR3 SDRAM.</summary>
  DDR3 = 0x18,

  /// <summary>FBD2 (Fully Buffered DIMM 2).</summary>
  FBD2 = 0x19,

  /// <summary>DDR4 SDRAM.</summary>
  DDR4 = 0x1a,

  /// <summary>LPDDR (Low-Power DDR) SDRAM.</summary>
  LPDDR = 0x1b,

  /// <summary>LPDDR2 SDRAM.</summary>
  LPDDR2 = 0x1c,

  /// <summary>LPDDR3 SDRAM.</summary>
  LPDDR3 = 0x1d,

  /// <summary>LPDDR4 SDRAM.</summary>
  LPDDR4 = 0x1e,

  /// <summary>A logical non-volatile device.</summary>
  LogicalNonVolatileDevice = 0x1f,

  /// <summary>HBM (High Bandwidth Memory).</summary>
  HBM = 0x20,

  /// <summary>HBM2 (High Bandwidth Memory 2).</summary>
  HBM2 = 0x21,

  /// <summary>DDR5 SDRAM.</summary>
  DDR5 = 0x22,

  /// <summary>LPDDR5 SDRAM.</summary>
  LPDDR5 = 0x23
}

/// <summary>
/// Base class providing helpers for reading typed values and strings from raw SMBIOS structure data.
/// </summary>
public class InformationBase {
  private readonly byte[] _data;
  private readonly IList<string> _strings;

  /// <summary>
  /// Initializes a new instance of the <see cref="InformationBase" /> class.
  /// </summary>
  /// <param name="data">The data.</param>
  /// <param name="strings">The strings.</param>
  protected InformationBase(byte[] data, IList<string> strings) {
    _data = data;
    _strings = strings;
  }

  /// <summary>
  /// Gets the byte.
  /// </summary>
  /// <param name="offset">The offset.</param>
  /// <returns><see cref="byte" />.</returns>
  protected byte GetByte(int offset) {
    if (offset < _data.Length && offset >= 0)
      return _data[offset];

    return 0;
  }

  /// <summary>
  /// Gets the word.
  /// </summary>
  /// <param name="offset">The offset.</param>
  /// <returns><see cref="ushort" />.</returns>
  protected ushort GetWord(int offset) {
    if (offset + 1 < _data.Length && offset >= 0) {
      return BitConverter.ToUInt16(_data, offset);
    }

    return 0;
  }

  /// <summary>
  /// Gets the dword.
  /// </summary>
  /// <param name="offset">The offset.</param>
  /// <returns><see cref="ushort" />.</returns>
  protected uint GetDword(int offset) {
    if (offset + 3 < _data.Length && offset >= 0) {
      return BitConverter.ToUInt32(_data, offset);
    }

    return 0;
  }

  /// <summary>
  /// Gets the qword.
  /// </summary>
  /// <param name="offset">The offset.</param>
  /// <returns><see cref="ulong" />.</returns>
  protected ulong GetQword(int offset) {
    if (offset + 7 < _data.Length && offset >= 0) {
      return BitConverter.ToUInt64(_data, offset);
    }

    return 0;
  }

  /// <summary>
  /// Gets the string.
  /// </summary>
  /// <param name="offset">The offset.</param>
  /// <returns><see cref="string" />.</returns>
  protected string GetString(int offset) {
    if (offset < _data.Length && _data[offset] > 0 && _data[offset] <= _strings.Count)
      return _strings[_data[offset] - 1];

    return string.Empty;
  }
}

/// <summary>
/// Motherboard BIOS information obtained from the SMBIOS table.
/// </summary>
public class BiosInformation : InformationBase {
  internal BiosInformation(string vendor, string version, string date = null, ulong? size = null) : base(null, null) {
    Vendor = vendor;
    Version = version;
    Date = GetDate(date);
    Size = size;
  }

  internal BiosInformation(byte[] data, IList<string> strings) : base(data, strings) {
    Vendor = GetString(0x04);
    Version = GetString(0x05);
    Date = GetDate(GetString(0x08));
    Size = GetSize();
  }

  /// <summary>
  /// Gets the BIOS release date.
  /// </summary>
  public DateTime? Date { get; }

  /// <summary>
  /// Gets the size of the physical device containing the BIOS.
  /// </summary>
  public ulong? Size { get; }

  /// <summary>
  /// Gets the string number of the BIOS Vendor’s Name.
  /// </summary>
  public string Vendor { get; }

  /// <summary>
  /// Gets the string number of the BIOS Version. This value is a free-form string that may contain Core and OEM version information.
  /// </summary>
  public string Version { get; }

  /// <summary>
  /// Gets the size.
  /// </summary>
  /// <returns><see cref="Nullable{Int64}" />.</returns>
  private ulong? GetSize() {
    int biosRomSize = GetByte(0x09);
    ushort extendedBiosRomSize = GetWord(0x18);

    bool isExtendedBiosRomSize = biosRomSize == 0xFF && extendedBiosRomSize != 0;
    if (!isExtendedBiosRomSize)
      return 65536 * (ulong)(biosRomSize + 1);

    int unit = (extendedBiosRomSize & 0xC000) >> 14;
    ulong extendedSize = (ulong)(extendedBiosRomSize & ~0xC000) * 1024 * 1024;

    switch (unit) {
      case 0x00: return extendedSize; // Megabytes
      case 0x01: return extendedSize * 1024; // Gigabytes - might overflow in the future
    }

    return null; // Other patterns not defined in DMI 3.2.0
  }

  /// <summary>
  /// Gets the date.
  /// </summary>
  /// <param name="date">The bios date.</param>
  /// <returns><see cref="Nullable{DateTime}" />.</returns>
  private static DateTime? GetDate(string date) {
    string[] parts = (date ?? string.Empty).Split('/');

    if (parts.Length == 3 &&
        int.TryParse(parts[0], out int month) &&
        int.TryParse(parts[1], out int day) &&
        int.TryParse(parts[2], out int year)) {
      // Check if the SMBIOS specification is followed.
      if (month > 12 || day > 31)
        return null;

      return new DateTime(year < 100 ? 1900 + year : year, month, day);
    }

    return null;
  }
}

/// <summary>
/// System information obtained from the SMBIOS table.
/// </summary>
public class SystemInformation : InformationBase {
  internal SystemInformation
  (
      string manufacturerName,
      string productName,
      string version,
      string serialNumber,
      string family,
      SystemWakeUp wakeUp = SystemWakeUp.Unknown) : base(null, null) {
    ManufacturerName = manufacturerName;
    ProductName = productName;
    Version = version;
    SerialNumber = serialNumber;
    Family = family;
    WakeUp = wakeUp;
  }

  internal SystemInformation(byte[] data, IList<string> strings) : base(data, strings) {
    ManufacturerName = GetString(0x04);
    ProductName = GetString(0x05);
    Version = GetString(0x06);
    SerialNumber = GetString(0x07);
    Family = GetString(0x1A);
    WakeUp = (SystemWakeUp)GetByte(0x18);
  }

  /// <summary>
  /// Gets the family associated with system.
  /// <para>
  /// This text string identifies the family to which a particular computer belongs. A family refers to a set of computers that are similar but not identical from a hardware or software point of
  /// view. Typically, a family is composed of different computer models, which have different configurations and pricing points. Computers in the same family often have similar branding and cosmetic
  /// features.
  /// </para>
  /// </summary>
  public string Family { get; }

  /// <summary>
  /// Gets the manufacturer name associated with system.
  /// </summary>
  public string ManufacturerName { get; }

  /// <summary>
  /// Gets the product name associated with system.
  /// </summary>
  public string ProductName { get; }

  /// <summary>
  /// Gets the serial number string associated with system.
  /// </summary>
  public string SerialNumber { get; }

  /// <summary>
  /// Gets the version string associated with system.
  /// </summary>
  public string Version { get; }

  /// <summary>
  /// Gets <inheritdoc cref="SystemWakeUp" />
  /// </summary>
  public SystemWakeUp WakeUp { get; }
}

/// <summary>
/// System enclosure obtained from the SMBIOS table.
/// </summary>
public class SystemEnclosure : InformationBase {
  internal SystemEnclosure(byte[] data, IList<string> strings) : base(data, strings) {
    ManufacturerName = GetString(0x04).Trim();
    Version = GetString(0x06).Trim();
    SerialNumber = GetString(0x07).Trim();
    AssetTag = GetString(0x08).Trim();
    RackHeight = GetByte(0x11);
    PowerCords = GetByte(0x12);
    SKU = GetString(0x15).Trim();
    LockDetected = (GetByte(0x05) & 128) == 128;
    Type = (SystemEnclosureType)(GetByte(0x05) & 127);
    BootUpState = (SystemEnclosureState)GetByte(0x09);
    PowerSupplyState = (SystemEnclosureState)GetByte(0x0A);
    ThermalState = (SystemEnclosureState)GetByte(0x0B);
    SecurityStatus = (SystemEnclosureSecurityStatus)GetByte(0x0C);
  }

  /// <summary>
  /// Gets the asset tag associated with the enclosure or chassis.
  /// </summary>
  public string AssetTag { get; }

  /// <summary>
  /// Gets <inheritdoc cref="SystemEnclosureState" />
  /// </summary>
  public SystemEnclosureState BootUpState { get; }

  /// <summary>
  /// Gets or sets the system enclosure lock.
  /// </summary>
  /// <returns>System enclosure lock is present if <see langword="true" />. Otherwise, either a lock is not present or it is unknown if the enclosure has a lock.</returns>
  public bool LockDetected { get; set; }

  /// <summary>
  /// Gets the string describing the chassis or enclosure manufacturer name.
  /// </summary>
  public string ManufacturerName { get; }

  /// <summary>
  /// Gets the number of power cords associated with the enclosure or chassis.
  /// </summary>
  public byte PowerCords { get; }

  /// <summary>
  /// Gets the state of the enclosure’s power supply (or supplies) when last booted.
  /// </summary>
  public SystemEnclosureState PowerSupplyState { get; }

  /// <summary>
  /// Gets the height of the enclosure, in 'U's. A U is a standard unit of measure for the height of a rack or rack-mountable component and is equal to 1.75 inches or 4.445 cm. A value of <c>0</c>
  /// indicates that the enclosure height is unspecified.
  /// </summary>
  public byte RackHeight { get; }

  /// <summary>
  /// Gets the physical security status of the enclosure when last booted.
  /// </summary>
  public SystemEnclosureSecurityStatus SecurityStatus { get; set; }

  /// <summary>
  /// Gets the string describing the chassis or enclosure serial number.
  /// </summary>
  public string SerialNumber { get; }

  /// <summary>
  /// Gets the string describing the chassis or enclosure SKU number.
  /// </summary>
  public string SKU { get; }

  /// <summary>
  /// Gets the thermal state of the enclosure when last booted.
  /// </summary>
  public SystemEnclosureState ThermalState { get; }

  /// <summary>
  /// Gets <inheritdoc cref="Type" />
  /// </summary>
  public SystemEnclosureType Type { get; }

  /// <summary>
  /// Gets the number of null-terminated string representing the chassis or enclosure version.
  /// </summary>
  public string Version { get; }
}

/// <summary>
/// Motherboard information obtained from the SMBIOS table.
/// </summary>
public class BaseBoardInformation : InformationBase {
  internal BaseBoardInformation(string manufacturerName, string productName, string version, string serialNumber) : base(null, null) {
    ManufacturerName = manufacturerName;
    ProductName = productName;
    Version = version;
    SerialNumber = serialNumber;
  }

  internal BaseBoardInformation(byte[] data, IList<string> strings) : base(data, strings) {
    ManufacturerName = GetString(0x04).Trim();
    ProductName = GetString(0x05).Trim();
    Version = GetString(0x06).Trim();
    SerialNumber = GetString(0x07).Trim();
  }

  /// <summary>
  /// Gets the value that represents the manufacturer's name.
  /// </summary>
  public string ManufacturerName { get; }

  /// <summary>
  /// Gets the value that represents the motherboard's name.
  /// </summary>
  public string ProductName { get; }

  /// <summary>
  /// Gets the value that represents the motherboard's serial number.
  /// </summary>
  public string SerialNumber { get; }

  /// <summary>
  /// Gets the value that represents the motherboard's revision number.
  /// </summary>
  public string Version { get; }
}

/// <summary>
/// Processor information obtained from the SMBIOS table.
/// </summary>
public class ProcessorInformation : InformationBase {
  internal ProcessorInformation(byte[] data, IList<string> strings) : base(data, strings) {
    SocketDesignation = GetString(0x04).Trim();
    ManufacturerName = GetString(0x07).Trim();
    Version = GetString(0x10).Trim();
    CoreCount = GetByte(0x23) != 255 ? GetByte(0x23) : GetWord(0x2A);
    CoreEnabled = GetByte(0x24) != 255 ? GetByte(0x24) : GetWord(0x2C);
    ThreadCount = GetByte(0x25) != 255 ? GetByte(0x25) : GetWord(0x2E);
    ExternalClock = GetWord(0x12);
    MaxSpeed = GetWord(0x14);
    CurrentSpeed = GetWord(0x16);
    Serial = GetString(0x20).Trim();
    Id = GetQword(0x08);
    Handle = GetWord(0x02);

    byte characteristics1 = GetByte(0x26);
    byte characteristics2 = GetByte(0x27);

    Characteristics = ProcessorCharacteristics.None;
    if (IsBitSet(characteristics1, 2))
      Characteristics |= ProcessorCharacteristics._64BitCapable;

    if (IsBitSet(characteristics1, 3))
      Characteristics |= ProcessorCharacteristics.MultiCore;

    if (IsBitSet(characteristics1, 4))
      Characteristics |= ProcessorCharacteristics.HardwareThread;

    if (IsBitSet(characteristics1, 5))
      Characteristics |= ProcessorCharacteristics.ExecuteProtection;

    if (IsBitSet(characteristics1, 6))
      Characteristics |= ProcessorCharacteristics.EnhancedVirtualization;

    if (IsBitSet(characteristics1, 7))
      Characteristics |= ProcessorCharacteristics.PowerPerformanceControl;

    if (IsBitSet(characteristics2, 0))
      Characteristics |= ProcessorCharacteristics._128BitCapable;

    ProcessorType = (ProcessorType)GetByte(0x05);
    Socket = (ProcessorSocket)GetByte(0x19);

    int family = GetByte(0x06);
    Family = (ProcessorFamily)(family == 254 ? GetWord(0x28) : family);

    L1CacheHandle = GetWord(0x1A);
    L2CacheHandle = GetWord(0x1C);
    L3CacheHandle = GetWord(0x1E);

    bool IsBitSet(byte b, int pos) {
      return (b & (1 << pos)) != 0;
    }
  }

  /// <summary>
  /// Gets the characteristics of the processor.
  /// </summary>
  public ProcessorCharacteristics Characteristics { get; }

  /// <summary>
  /// Gets the value that represents the number of cores per processor socket.
  /// </summary>
  public ushort CoreCount { get; }

  /// <summary>
  /// Gets the value that represents the number of enabled cores per processor socket.
  /// </summary>
  public ushort CoreEnabled { get; }

  /// <summary>
  /// Gets the value that represents the current processor speed (in MHz).
  /// </summary>
  public ushort CurrentSpeed { get; }

  /// <summary>
  /// Gets the external Clock Frequency, in MHz. If the value is unknown, the field is set to 0.
  /// </summary>
  public ushort ExternalClock { get; }

  /// <summary>
  /// Gets <inheritdoc cref="ProcessorFamily" />
  /// </summary>
  public ProcessorFamily Family { get; }

  /// <summary>
  /// Gets the handle.
  /// </summary>
  /// <value>The handle.</value>
  public ushort Handle { get; }

  /// <summary>
  /// Gets the identifier.
  /// </summary>
  public ulong Id { get; }

  /// <summary>
  /// Gets the L1 cache handle.
  /// </summary>
  public ushort L1CacheHandle { get; }

  /// <summary>
  /// Gets the L2 cache handle.
  /// </summary>
  public ushort L2CacheHandle { get; }

  /// <summary>
  /// Gets the L3 cache handle.
  /// </summary>
  public ushort L3CacheHandle { get; }

  /// <summary>
  /// Gets the string number of Processor Manufacturer.
  /// </summary>
  public string ManufacturerName { get; }

  /// <summary>
  /// Gets the value that represents the maximum processor speed (in MHz) supported by the system for this processor socket.
  /// </summary>
  public ushort MaxSpeed { get; }

  /// <summary>
  /// Gets <inheritdoc cref="Crystal.Provider.Telemetry.Hardware.ProcessorType" />
  /// </summary>
  public ProcessorType ProcessorType { get; }

  /// <summary>
  /// Gets the value that represents the string number for the serial number of this processor.
  /// <para>This value is set by the manufacturer and normally not changeable.</para>
  /// </summary>
  public string Serial { get; }

  /// <summary>
  /// Gets <inheritdoc cref="ProcessorSocket" />
  /// </summary>
  public ProcessorSocket Socket { get; }

  /// <summary>
  /// Gets the string number for Reference Designation.
  /// </summary>
  public string SocketDesignation { get; }

  /// <summary>
  /// Gets the value that represents the number of threads per processor socket.
  /// </summary>
  public ushort ThreadCount { get; }

  /// <summary>
  /// Gets the value that represents the string number describing the Processor.
  /// </summary>
  public string Version { get; }
}

/// <summary>
/// Cache information obtained from the SMBIOS table.
/// </summary>
public class CacheInformation : InformationBase {
  internal CacheInformation(byte[] data, IList<string> strings) : base(data, strings) {
    Handle = GetWord(0x02);
    Designation = GetCacheDesignation();
    Associativity = (CacheAssociativity)GetByte(0x12);
    Size = GetWord(0x09);
  }

  /// <summary>
  /// Gets <inheritdoc cref="CacheAssociativity" />
  /// </summary>
  public CacheAssociativity Associativity { get; }

  /// <summary>
  /// Gets <inheritdoc cref="CacheDesignation" />
  /// </summary>
  public CacheDesignation Designation { get; }

  /// <summary>
  /// Gets the handle.
  /// </summary>
  public ushort Handle { get; }

  /// <summary>
  /// Gets the value that represents the installed cache size.
  /// </summary>
  public ushort Size { get; }

  /// <summary>
  /// Gets the cache designation.
  /// </summary>
  /// <returns><see cref="CacheDesignation" />.</returns>
  private CacheDesignation GetCacheDesignation() {
    string rawCacheType = GetString(0x04);

    if (rawCacheType.Contains("L1"))
      return CacheDesignation.L1;

    if (rawCacheType.Contains("L2"))
      return CacheDesignation.L2;

    if (rawCacheType.Contains("L3"))
      return CacheDesignation.L3;

    return CacheDesignation.Other;
  }
}

/// <summary>
/// Memory information obtained from the SMBIOS table.
/// </summary>
public class MemoryDevice : InformationBase {
  internal MemoryDevice(byte[] data, IList<string> strings) : base(data, strings) {
    DeviceLocator = GetString(0x10).Trim();
    BankLocator = GetString(0x11).Trim();
    ManufacturerName = GetString(0x17).Trim();
    SerialNumber = GetString(0x18).Trim();
    PartNumber = GetString(0x1A).Trim();
    Speed = GetWord(0x15);
    ConfiguredSpeed = GetWord(0x20);
    ConfiguredVoltage = GetWord(0x26);
    Size = GetWord(0x0C);
    if (Size == 0x7FFF)
      Size = GetDword(0x1C);
    Type = (MemoryType)GetByte(0x12);
  }

  /// <summary>
  /// Gets the string number of the string that identifies the physically labeled bank where the memory device is located.
  /// </summary>
  public string BankLocator { get; }

  /// <summary>
  /// Gets the string number of the string that identifies the physically-labeled socket or board position where the memory device is located.
  /// </summary>
  public string DeviceLocator { get; }

  /// <summary>
  /// Gets the string number for the manufacturer of this memory device.
  /// </summary>
  public string ManufacturerName { get; }

  /// <summary>
  /// Gets the string number for the part number of this memory device.
  /// </summary>
  public string PartNumber { get; }

  /// <summary>
  /// Gets the string number for the serial number of this memory device.
  /// </summary>
  public string SerialNumber { get; }

  /// <summary>
  /// Gets the size of the memory device.
  /// If the value is 0, no memory device is installed in the socket.
  /// If the value is 0xFFFF, the size is unknown.
  /// </summary>
  public uint Size { get; }

  /// <summary>
  /// Gets the value that identifies the maximum capable speed of the device, in mega transfers per second (MT/s).
  /// </summary>
  public ushort Speed { get; }

  /// <summary>
  /// Gets the configured speed of the device, in mega transfers per second (MT/s).
  /// </summary>
  public ushort ConfiguredSpeed { get; }

  /// <summary>
  /// Gets the configured voltage of this memory device, in millivolts (mV).
  /// </summary>
  public ushort ConfiguredVoltage { get; }

  /// <summary>
  /// Gets the type of this memory device.
  /// </summary>
  /// <value>The type.</value>
  public MemoryType Type { get; }
}

/// <summary>
/// Reads and processes information encoded in an SMBIOS table.
/// </summary>
public class SMBios {
  private readonly byte[] _raw;
  private readonly Version _version;

  /// <summary>
  /// Initializes a new instance of the <see cref="SMBios" /> class.
  /// </summary>
  public SMBios() {
    if (Software.OperatingSystem.IsUnix) {
      _raw = null;

      string boardVendor = ReadSysFs("/sys/class/dmi/id/board_vendor");
      string boardName = ReadSysFs("/sys/class/dmi/id/board_name");
      string boardVersion = ReadSysFs("/sys/class/dmi/id/board_version");
      Board = new BaseBoardInformation(boardVendor, boardName, boardVersion, null);

      string systemVendor = ReadSysFs("/sys/class/dmi/id/sys_vendor");
      string productName = ReadSysFs("/sys/class/dmi/id/product_name");
      string productVersion = ReadSysFs("/sys/class/dmi/id/product_version");
      System = new SystemInformation(systemVendor, productName, productVersion, null, null);

      string biosVendor = ReadSysFs("/sys/class/dmi/id/bios_vendor");
      string biosVersion = ReadSysFs("/sys/class/dmi/id/bios_version");
      string biosDate = ReadSysFs("/sys/class/dmi/id/bios_date");
      Bios = new BiosInformation(biosVendor, biosVersion, biosDate);

      MemoryDevices = Array.Empty<MemoryDevice>();
      ProcessorCaches = Array.Empty<CacheInformation>();
    }
    else {
      List<MemoryDevice> memoryDeviceList = new();
      List<CacheInformation> processorCacheList = new();
      List<ProcessorInformation> processorInformationList = new();

      string[] tables = FirmwareTable.EnumerateTables(FIRMWARE_TABLE_PROVIDER.RSMB);
      if (tables is { Length: > 0 }) {
        _raw = FirmwareTable.GetTable(FIRMWARE_TABLE_PROVIDER.RSMB, tables[0]);
        if (_raw == null || _raw.Length == 0)
          return;

        byte majorVersion = _raw[1];
        byte minorVersion = _raw[2];

        if (majorVersion > 0 || minorVersion > 0)
          _version = new Version(majorVersion, minorVersion);

        if (_raw is { Length: > 0 }) {
          int offset = 8;
          byte type = _raw[offset];

          while (offset + 4 < _raw.Length && type != 127) {
            type = _raw[offset];
            int length = _raw[offset + 1];

            if (offset + length > _raw.Length)
              break;

            byte[] data = new byte[length];
            Array.Copy(_raw, offset, data, 0, length);
            offset += length;

            List<string> strings = new();
            if (offset < _raw.Length && _raw[offset] == 0)
              offset++;

            while (offset < _raw.Length && _raw[offset] != 0) {
              StringBuilder stringBuilder = new();

              while (offset < _raw.Length && _raw[offset] != 0) {
                stringBuilder.Append((char)_raw[offset]);
                offset++;
              }

              offset++;

              strings.Add(stringBuilder.ToString());
            }

            offset++;
            switch (type) {
              case 0x00:
                Bios = new BiosInformation(data, strings);
                break;
              case 0x01:
                System = new SystemInformation(data, strings);
                break;
              case 0x02:
                Board = new BaseBoardInformation(data, strings);
                break;
              case 0x03:
                SystemEnclosure = new SystemEnclosure(data, strings);
                break;
              case 0x04:
                processorInformationList.Add(new ProcessorInformation(data, strings));
                break;
              case 0x07:
                processorCacheList.Add(new CacheInformation(data, strings));
                break;
              case 0x11:
                memoryDeviceList.Add(new MemoryDevice(data, strings));
                break;
            }
          }
        }
      }

      MemoryDevices = memoryDeviceList.ToArray();
      ProcessorCaches = processorCacheList.ToArray();
      Processors = processorInformationList.ToArray();
    }
  }

  /// <summary>
  /// Gets <inheritdoc cref="BiosInformation" />
  /// </summary>
  public BiosInformation Bios { get; }

  /// <summary>
  /// Gets <inheritdoc cref="BaseBoardInformation" />
  /// </summary>
  public BaseBoardInformation Board { get; }

  /// <summary>
  /// Gets <inheritdoc cref="MemoryDevice" />
  /// </summary>
  public MemoryDevice[] MemoryDevices { get; }

  /// <summary>
  /// Gets <inheritdoc cref="CacheInformation" />
  /// </summary>
  public CacheInformation[] ProcessorCaches { get; }

  /// <summary>
  /// Gets <inheritdoc cref="ProcessorInformation" />
  /// </summary>
  public ProcessorInformation[] Processors { get; }

  /// <summary>
  /// Gets <inheritdoc cref="SystemInformation" />
  /// </summary>
  public SystemInformation System { get; }

  /// <summary>
  /// Gets <inheritdoc cref="Crystal.Provider.Telemetry.Hardware.SystemEnclosure" />
  /// </summary>
  public SystemEnclosure SystemEnclosure { get; }

  private static string ReadSysFs(string path) {
    try {
      if (File.Exists(path)) {
        using StreamReader reader = new(path);

        return reader.ReadLine();
      }

      return string.Empty;
    }
    catch {
      return string.Empty;
    }
  }

  /// <summary>
  /// Report containing most of the information that could be read from the SMBIOS table.
  /// </summary>
  /// <returns>A formatted text string with computer information and the entire SMBIOS table.</returns>
  public string GetReport() {
    StringBuilder r = new();

    if (_version != null) {
      r.Append("SMBios Version: ");
      r.AppendLine(_version.ToString(2));
      r.AppendLine();
    }

    if (Bios != null) {
      r.Append("BIOS Vendor: ");
      r.AppendLine(Bios.Vendor);
      r.Append("BIOS Version: ");
      r.AppendLine(Bios.Version);
      if (Bios.Date != null) {
        r.Append("BIOS Date: ");
        r.AppendLine(Bios.Date.Value.ToShortDateString());
      }

      if (Bios.Size != null) {
        const int megabyte = 1024 * 1024;
        r.Append("BIOS Size: ");
        if (Bios.Size > megabyte)
          r.AppendLine((Bios.Size.Value / megabyte) + " MB");
        else
          r.AppendLine((Bios.Size.Value / 1024) + " KB");
      }

      r.AppendLine();
    }

    if (System != null) {
      r.Append("System Manufacturer: ");
      r.AppendLine(System.ManufacturerName);
      r.Append("System Name: ");
      r.AppendLine(System.ProductName);
      r.Append("System Version: ");
      r.AppendLine(System.Version);
      r.Append("System Wakeup: ");
      r.AppendLine(System.WakeUp.ToString());
      r.AppendLine();
    }

    if (Board != null) {
      r.Append("Motherboard Manufacturer: ");
      r.AppendLine(Board.ManufacturerName);
      r.Append("Motherboard Name: ");
      r.AppendLine(Board.ProductName);
      r.Append("Motherboard Version: ");
      r.AppendLine(Board.Version);
      r.Append("Motherboard Serial: ");
      r.AppendLine(Board.SerialNumber);
      r.AppendLine();
    }

    if (SystemEnclosure != null) {
      r.Append("System Enclosure Type: ");
      r.AppendLine(SystemEnclosure.Type.ToString());
      r.Append("System Enclosure Manufacturer: ");
      r.AppendLine(SystemEnclosure.ManufacturerName);
      r.Append("System Enclosure Version: ");
      r.AppendLine(SystemEnclosure.Version);
      r.Append("System Enclosure Serial: ");
      r.AppendLine(SystemEnclosure.SerialNumber);
      r.Append("System Enclosure Asset Tag: ");
      r.AppendLine(SystemEnclosure.AssetTag);
      if (!string.IsNullOrEmpty(SystemEnclosure.SKU)) {
        r.Append("System Enclosure SKU: ");
        r.AppendLine(SystemEnclosure.SKU);
      }

      r.Append("System Enclosure Boot Up State: ");
      r.AppendLine(SystemEnclosure.BootUpState.ToString());
      r.Append("System Enclosure Power Supply State: ");
      r.AppendLine(SystemEnclosure.PowerSupplyState.ToString());
      r.Append("System Enclosure Thermal State: ");
      r.AppendLine(SystemEnclosure.ThermalState.ToString());
      r.Append("System Enclosure Power Cords: ");
      r.AppendLine(SystemEnclosure.PowerCords.ToString());
      if (SystemEnclosure.RackHeight > 0) {
        r.Append("System Enclosure Rack Height: ");
        r.AppendLine(SystemEnclosure.RackHeight.ToString());
      }

      r.Append("System Enclosure Lock Detected: ");
      r.AppendLine(SystemEnclosure.LockDetected ? "Yes" : "No");
      r.Append("System Enclosure Security Status: ");
      r.AppendLine(SystemEnclosure.SecurityStatus.ToString());
      r.AppendLine();
    }

    if (Processors != null) {
      foreach (ProcessorInformation processor in Processors) {
        r.Append("Processor Manufacturer: ");
        r.AppendLine(processor.ManufacturerName);
        r.Append("Processor Type: ");
        r.AppendLine(processor.ProcessorType.ToString());
        r.Append("Processor Version: ");
        r.AppendLine(processor.Version);
        r.Append("Processor Serial: ");
        r.AppendLine(processor.Serial);
        r.Append("Processor Socket Designation: ");
        r.AppendLine(processor.SocketDesignation);
        r.Append("Processor Socket: ");
        r.AppendLine(processor.Socket.ToString());
        r.Append("Processor Version: ");
        r.AppendLine(processor.Version);
        r.Append("Processor Family: ");
        r.AppendLine(processor.Family.ToString());
        r.Append("Processor Core Count: ");
        r.AppendLine(processor.CoreCount.ToString());
        r.Append("Processor Core Enabled: ");
        r.AppendLine(processor.CoreEnabled.ToString());
        r.Append("Processor Thread Count: ");
        r.AppendLine(processor.ThreadCount.ToString());
        r.Append("Processor External Clock: ");
        r.Append(processor.ExternalClock);
        r.AppendLine(" Mhz");
        r.Append("Processor Max Speed: ");
        r.Append(processor.MaxSpeed);
        r.AppendLine(" Mhz");
        r.Append("Processor Current Speed: ");
        r.Append(processor.CurrentSpeed);
        r.AppendLine(" Mhz");
        r.AppendLine();
      }
    }

    if (ProcessorCaches != null) {
      foreach (CacheInformation processorCaches in ProcessorCaches) {
        r.Append("Cache [" + processorCaches.Designation + "] Size: ");
        r.AppendLine(processorCaches.Size.ToString());
        r.Append("Cache [" + processorCaches.Designation + "] Associativity: ");
        r.AppendLine(processorCaches.Associativity.ToString().Replace("_", string.Empty));
        r.AppendLine();
      }
    }

    for (int i = 0; i < MemoryDevices.Length; i++) {
      r.Append("Memory Device [" + i + "] Manufacturer: ");
      r.AppendLine(MemoryDevices[i].ManufacturerName);
      r.Append("Memory Device [" + i + "] Part Number: ");
      r.AppendLine(MemoryDevices[i].PartNumber);
      r.Append("Memory Device [" + i + "] Device Locator: ");
      r.AppendLine(MemoryDevices[i].DeviceLocator);
      r.Append("Memory Device [" + i + "] Bank Locator: ");
      r.AppendLine(MemoryDevices[i].BankLocator);
      r.Append("Memory Device [" + i + "] Speed: ");
      r.AppendLine(MemoryDevices[i].Speed.ToString());
      r.Append("Memory Device [" + i + "] Configured Speed: ");
      r.AppendLine(MemoryDevices[i].ConfiguredSpeed.ToString());
      r.Append("Memory Device [" + i + "] Configured Voltage: ");
      r.AppendLine(MemoryDevices[i].ConfiguredVoltage.ToString());
      r.Append("Memory Device [" + i + "] Size: ");
      r.Append(MemoryDevices[i].Size.ToString());
      r.AppendLine(" MB");
      r.AppendLine();
    }

    if (_raw != null) {
      string base64 = Convert.ToBase64String(_raw);
      r.AppendLine("SMBios Table");
      r.AppendLine();

      for (int i = 0; i < Math.Ceiling(base64.Length / 64.0); i++) {
        r.Append(" ");
        for (int j = 0; j < 0x40; j++) {
          int index = (i << 6) | j;
          if (index < base64.Length) {
            r.Append(base64[index]);
          }
        }

        r.AppendLine();
      }

      r.AppendLine();
    }

    return r.ToString();
  }
}
