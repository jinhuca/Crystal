namespace Crystal.Mmi.HardwareFeatures.SerialPortConfiguration;

internal static class WmiSerialPortConfiguration
{
    public const string ClassName = "Win32_SerialPortConfiguration";

    public const string Caption = nameof(Caption);
    public const string Description = nameof(Description);
    public const string Name = nameof(Name);
    public const string SettingID = nameof(SettingID);
    public const string AbortReadWriteOnError = nameof(AbortReadWriteOnError);
    public const string BaudRate = nameof(BaudRate);
    public const string Binary = nameof(Binary);
    public const string BitsPerByte = nameof(BitsPerByte);
    public const string ContinueXMitOnXOff = nameof(ContinueXMitOnXOff);
    public const string CTSOutflowControl = nameof(CTSOutflowControl);
    public const string DiscardNULL = nameof(DiscardNULL);
    public const string DSROutflowControl = nameof(DSROutflowControl);
    public const string DSRSensitivity = nameof(DSRSensitivity);
    public const string DTRFlowControlType = nameof(DTRFlowControlType);
    public const string EOFCharacter = nameof(EOFCharacter);
    public const string ErrorReplaceCharacter = nameof(ErrorReplaceCharacter);
    public const string InFlowControlType = nameof(InFlowControlType);
    public const string OutFlowControlType = nameof(OutFlowControlType);
    public const string Parity = nameof(Parity);
    public const string ParityCheck = nameof(ParityCheck);
    public const string RTSFlowControlType = nameof(RTSFlowControlType);
    public const string StopBits = nameof(StopBits);
    public const string XOffCharacter = nameof(XOffCharacter);
    public const string XOffXMitThreshold = nameof(XOffXMitThreshold);
    public const string XOnCharacter = nameof(XOnCharacter);
    public const string XOnXMitThreshold = nameof(XOnXMitThreshold);
    public const string XOnXOffInFlowControl = nameof(XOnXOffInFlowControl);
    public const string XOnXOffOutFlowControl = nameof(XOnXOffOutFlowControl);
}
