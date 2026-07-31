namespace Crystal.Telemetry.Interop.PowerMonitor;

/// <summary>
/// USB command opcodes sent to or received from the WireView Pro 2 device.
/// </summary>
public enum UsbCmd : byte {
  /// <summary>Initial welcome/handshake command.</summary>
  CMD_WELCOME,
  /// <summary>Reads vendor-specific data from the device.</summary>
  CMD_READ_VENDOR_DATA,
  /// <summary>Reads the device's unique identifier (UID).</summary>
  CMD_READ_UID,
  /// <summary>Reads general device data.</summary>
  CMD_READ_DEVICE_DATA,
  /// <summary>Reads the current sensor values.</summary>
  CMD_READ_SENSOR_VALUES,
  /// <summary>Reads the device configuration.</summary>
  CMD_READ_CONFIG,
  /// <summary>Writes the device configuration.</summary>
  CMD_WRITE_CONFIG,
  /// <summary>Reads the calibration data.</summary>
  CMD_READ_CALIBRATION,
  /// <summary>Writes the calibration data.</summary>
  CMD_WRITE_CALIBRATION,
  /// <summary>Writes a page to the SPI flash memory.</summary>
  CMD_SPI_FLASH_WRITE_PAGE,
  /// <summary>Reads a page from the SPI flash memory.</summary>
  CMD_SPI_FLASH_READ_PAGE,
  /// <summary>Erases a sector of the SPI flash memory.</summary>
  CMD_SPI_FLASH_ERASE_SECTOR,
  /// <summary>Changes the currently displayed screen.</summary>
  CMD_SCREEN_CHANGE,
  /// <summary>Reads firmware build information.</summary>
  CMD_READ_BUILD_INFO,
  /// <summary>Clears any active fault flags.</summary>
  CMD_CLEAR_FAULTS,
  /// <summary>Resets the device.</summary>
  CMD_RESET = 0xF0,
  /// <summary>Enters the firmware bootloader.</summary>
  CMD_BOOTLOADER = 0xF1,
  /// <summary>Performs a non-volatile memory (NVM) configuration operation.</summary>
  CMD_NVM_CONFIG = 0xF2,
  /// <summary>No operation.</summary>
  CMD_NOP = 0xFF
}

/// <summary>
/// Identifies a temperature sensor channel on the device.
/// </summary>
public enum SensorTs {
  /// <summary>Input-side temperature sensor.</summary>
  SENSOR_TS_IN,
  /// <summary>Output-side temperature sensor.</summary>
  SENSOR_TS_OUT,
  /// <summary>Third temperature sensor.</summary>
  SENSOR_TS3,
  /// <summary>Fourth temperature sensor.</summary>
  SENSOR_TS4,
}

/// <summary>
/// High-power (HPWR) connector power-delivery capability rating.
/// </summary>
public enum HpwrCapability : byte {
  /// <summary>Power supply capable of 600 W.</summary>
  PSU_CAP_600W = 0,
  /// <summary>Power supply capable of 450 W.</summary>
  PSU_CAP_450W = 1,
  /// <summary>Power supply capable of 300 W.</summary>
  PSU_CAP_300W = 2,
  /// <summary>Power supply capable of 150 W.</summary>
  PSU_CAP_150W = 3
}

/// <summary>
/// Fan control mode.
/// </summary>
public enum FanMode : byte {
  /// <summary>Fan speed follows a temperature curve.</summary>
  FanModeCurve = 0,
  /// <summary>Fan runs at a fixed speed.</summary>
  FanModeFixed = 1
}

/// <summary>
/// Temperature source used for display or fan control.
/// </summary>
public enum TempSource : byte {
  /// <summary>Input-side temperature sensor.</summary>
  TempSourceTsIn = 0,
  /// <summary>Output-side temperature sensor.</summary>
  TempSourceTsOut = 1,
  /// <summary>First temperature sensor.</summary>
  TempSourceTs1 = 2,
  /// <summary>Second temperature sensor.</summary>
  TempSourceTs2 = 3,
  /// <summary>Maximum of all temperature sensors.</summary>
  TempSourceTmax = 4
}

/// <summary>
/// Full-scale range used for current display.
/// </summary>
public enum CurrentScale : byte {
  /// <summary>5 A full scale.</summary>
  CurrentScale5A = 0,
  /// <summary>10 A full scale.</summary>
  CurrentScale10A = 1,
  /// <summary>15 A full scale.</summary>
  CurrentScale15A = 2,
  /// <summary>20 A full scale.</summary>
  CurrentScale20A = 3
}

/// <summary>
/// Full-scale range used for power display.
/// </summary>
public enum PowerScale : byte {
  /// <summary>Automatic power scale selection.</summary>
  PowerScaleAuto = 0,
  /// <summary>300 W full scale.</summary>
  PowerScale300W = 1,
  /// <summary>600 W full scale.</summary>
  PowerScale600W = 2
}

/// <summary>
/// Display color theme.
/// </summary>
public enum Theme : byte {
  /// <summary>Theme variant 1.</summary>
  ThemeTg1 = 0,
  /// <summary>Theme variant 2.</summary>
  ThemeTg2 = 1,
  /// <summary>Theme variant 3.</summary>
  ThemeTg3 = 2
}

/// <summary>
/// Orientation of the display.
/// </summary>
public enum DisplayRotation : byte {
  /// <summary>No rotation (0 degrees).</summary>
  DisplayRotation0 = 0,
  /// <summary>Rotated 180 degrees.</summary>
  DisplayRotation180 = 1
}

/// <summary>
/// Behavior applied when the display timeout elapses.
/// </summary>
public enum TimeoutMode : byte {
  /// <summary>Display remains on a static screen.</summary>
  TimeoutModeStatic = 0,
  /// <summary>Display cycles through screens.</summary>
  TimeoutModeCycle = 1,
  /// <summary>Display enters sleep mode.</summary>
  TimeoutModeSleep = 2
}

/// <summary>
/// Identifies a display screen on the device.
/// </summary>
public enum Screen : byte {
  /// <summary>Main screen.</summary>
  ScreenMain = 0,
  /// <summary>Simplified screen.</summary>
  ScreenSimple = 1,
  /// <summary>Current readings screen.</summary>
  ScreenCurrent = 2,
  /// <summary>Temperature readings screen.</summary>
  ScreenTemp = 3,
  /// <summary>Status screen.</summary>
  ScreenStatus = 4
}

/// <summary>
/// Non-volatile memory (NVM) command.
/// </summary>
public enum NVM_CMD : byte {
  /// <summary>No operation.</summary>
  NVM_CMD_NONE,
  /// <summary>Loads configuration from NVM.</summary>
  NVM_CMD_LOAD,
  /// <summary>Stores configuration to NVM.</summary>
  NVM_CMD_STORE,
  /// <summary>Resets NVM to defaults.</summary>
  NVM_CMD_RESET,
  /// <summary>Loads calibration data from NVM.</summary>
  NVM_CMD_LOAD_CAL,
  /// <summary>Stores calibration data to NVM.</summary>
  NVM_CMD_STORE_CAL,
  /// <summary>Loads factory calibration data from NVM.</summary>
  NVM_CMD_LOAD_CAL_FACTORY,
  /// <summary>Stores factory calibration data to NVM.</summary>
  NVM_CMD_STORE_CAL_FACTORY,
}

/// <summary>
/// Screen navigation and update-control commands.
/// </summary>
public enum SCREEN_CMD : byte {
  /// <summary>Navigate to the main screen.</summary>
  SCREEN_GOTO_MAIN = 0xE0,
  /// <summary>Navigate to the simplified screen.</summary>
  SCREEN_GOTO_SIMPLE = 0xE1,
  /// <summary>Navigate to the current readings screen.</summary>
  SCREEN_GOTO_CURRENT = 0xE2,
  /// <summary>Navigate to the temperature readings screen.</summary>
  SCREEN_GOTO_TEMP = 0xE3,
  /// <summary>Navigate to the status screen.</summary>
  SCREEN_GOTO_STATUS = 0xE4,
  /// <summary>Remain on the current screen.</summary>
  SCREEN_GOTO_SAME = 0xEF,
  /// <summary>Pause screen updates.</summary>
  SCREEN_PAUSE_UPDATES = 0xF0,
  /// <summary>Resume screen updates.</summary>
  SCREEN_RESUME_UPDATES = 0xF1
}

/// <summary>
/// Averaging window applied to sensor sampling.
/// </summary>
public enum AVG : byte {
  /// <summary>22 ms averaging window.</summary>
  AVG_22MS,
  /// <summary>44 ms averaging window.</summary>
  AVG_44MS,
  /// <summary>89 ms averaging window.</summary>
  AVG_89MS,
  /// <summary>177 ms averaging window.</summary>
  AVG_177MS,
  /// <summary>354 ms averaging window.</summary>
  AVG_354MS,
  /// <summary>709 ms averaging window.</summary>
  AVG_709MS,
  /// <summary>1417 ms averaging window.</summary>
  AVG_1417MS,
}

/// <summary>
/// Display color inversion setting.
/// </summary>
public enum DISPLAY_INVERSION : byte {
  /// <summary>Color inversion disabled.</summary>
  DISPLAY_INVERSION_OFF,
  /// <summary>Color inversion enabled.</summary>
  DISPLAY_INVERSION_ON,
  /// <summary>Number of display inversion modes.</summary>
  DISPLAY_INVERSION_NUM
}

/// <summary>
/// Background theme applied to the display.
/// </summary>
public enum THEME_BACKGROUND : byte {
  /// <summary>Thermal Grizzly orange background.</summary>
  ThermalGrizzlyOrange = 1,
  /// <summary>Thermal Grizzly dark background.</summary>
  ThermalGrizzlyDark = 2,
  /// <summary>Background theme disabled.</summary>
  Disabled = 255
}

/// <summary>
/// Fan graphic theme applied to the display.
/// </summary>
public enum THEME_FAN : byte {
  /// <summary>Thermal Grizzly orange fan theme.</summary>
  ThermalGrizzlyOrange = 0x64, // Bitmap 4 + 6
  /// <summary>Thermal Grizzly dark fan theme.</summary>
  ThermalGrizzlyDark = 0x75, // Bitmap 5 + 7
  /// <summary>Thermal Grizzly black-and-white fan theme.</summary>
  ThermalGrizzlyBlackWhite = 0x98, // Bitmap 8 + 9
}
