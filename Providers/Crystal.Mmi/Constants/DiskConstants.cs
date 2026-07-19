using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class DiskConstants {
  public const string QueryString = "SELECT * FROM Win32_DiskDrive";

  public const string AvailabilityKey = "Availability";
  public const string AvailabilityDesc = "Availability and status of the disk drive";

  public const string BytesPerSectorKey = "BytesPerSector";
  public const string BytesPerSectorDesc = "Number of bytes in each physical sector of the disk drive";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the disk drive";

  public const string ConfigManagerErrorCodeKey = "ConfigManagerErrorCode";
  public const string ConfigManagerErrorCodeDesc = "Windows Plug and Play error code";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class to appear in the inheritance chain";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the disk drive";

  public const string DeviceIDKey = "DeviceID";
  public const string DeviceIDDesc = "Unique identifier of the disk drive, e.g. \\\\.\\PHYSICALDRIVE0";

  public const string FirmwareRevisionKey = "FirmwareRevision";
  public const string FirmwareRevisionDesc = "Firmware revision of the disk drive";

  public const string IndexKey = "Index";
  public const string IndexDesc = "Zero-based index of the disk drive, used to distinguish disks in a multi-disk system";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the disk drive was installed";

  public const string InterfaceTypeKey = "InterfaceType";
  public const string InterfaceTypeDesc = "Interface type of the disk drive (SCSI, IDE, USB, 1394, etc.)";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the disk drive's manufacturer";

  public const string MediaLoadedKey = "MediaLoaded";
  public const string MediaLoadedDesc = "If True, the media for the disk drive is loaded";

  public const string MediaTypeKey = "MediaType";
  public const string MediaTypeDesc = "Type of media used by the disk drive";

  public const string ModelKey = "Model";
  public const string ModelDesc = "Manufacturer's model number of the disk drive";

  public const string NameKey = "Name";
  public const string NameDesc = "Label by which the disk drive is known";

  public const string PartitionsKey = "Partitions";
  public const string PartitionsDesc = "Number of partitions on the disk drive that are recognized by the operating system";

  public const string PNPDeviceIDKey = "PNPDeviceID";
  public const string PNPDeviceIDDesc = "Windows Plug and Play device identifier of the disk drive";

  public const string SCSIBusKey = "SCSIBus";
  public const string SCSIBusDesc = "SCSI bus number of the disk drive";

  public const string SCSILogicalUnitKey = "SCSILogicalUnit";
  public const string SCSILogicalUnitDesc = "SCSI logical unit number (LUN) of the disk drive";

  public const string SCSIPortKey = "SCSIPort";
  public const string SCSIPortDesc = "SCSI port number of the disk drive";

  public const string SCSITargetIdKey = "SCSITargetId";
  public const string SCSITargetIdDesc = "SCSI target identifier of the disk drive";

  public const string SectorsPerTrackKey = "SectorsPerTrack";
  public const string SectorsPerTrackDesc = "Number of sectors in each track for the disk drive";

  public const string SerialNumberKey = "SerialNumber";
  public const string SerialNumberDesc = "Number allocated by the manufacturer to identify the physical media";

  public const string SignatureKey = "Signature";
  public const string SignatureDesc = "Disk identification signature, written to the master boot record";

  public const string SizeKey = "Size";
  public const string SizeDesc = "Size of the disk drive, in bytes";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the disk drive";

  public const string StatusInfoKey = "StatusInfo";
  public const string StatusInfoDesc = "State of the disk drive";

  public const string SystemCreationClassNameKey = "SystemCreationClassName";
  public const string SystemCreationClassNameDesc = "Value of the scoping computer system's CreationClassName property";

  public const string SystemNameKey = "SystemName";
  public const string SystemNameDesc = "Name of the scoping system";

  public const string TotalCylindersKey = "TotalCylinders";
  public const string TotalCylindersDesc = "Total number of cylinders on the physical disk drive";

  public const string TotalHeadsKey = "TotalHeads";
  public const string TotalHeadsDesc = "Total number of heads on the disk drive";

  public const string TotalSectorsKey = "TotalSectors";
  public const string TotalSectorsDesc = "Total number of sectors on the physical disk drive";

  public const string TotalTracksKey = "TotalTracks";
  public const string TotalTracksDesc = "Total number of tracks on the physical disk drive";

  public const string TracksPerCylinderKey = "TracksPerCylinder";
  public const string TracksPerCylinderDesc = "Number of tracks in each cylinder on the disk drive";

  public const string SizeUnit = "bytes";
}
