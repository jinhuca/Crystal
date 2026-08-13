using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;

/// <summary>
/// Parses the subset of the NoteBook FanControl (NBFC) <c>FanControlConfigV2</c> XML schema that
/// Crystal needs to read fan-speed percentage from a laptop's embedded controller. Uses
/// LINQ-to-XML (reflection-free) so it stays trim/AOT-safe.
/// </summary>
internal static class NbfcConfigParser {
  /// <summary>
  /// Parses an NBFC config from its XML text.
  /// </summary>
  /// <param name="xml">The NBFC config XML.</param>
  /// <returns>The parsed <see cref="NbfcFanConfig"/>, or <see langword="null"/> if the text is not a usable config.</returns>
  public static NbfcFanConfig Parse(string xml) {
    if (string.IsNullOrWhiteSpace(xml))
      return null;

    XDocument document;
    try {
      document = XDocument.Parse(xml);
    }
    catch (System.Xml.XmlException) {
      return null;
    }

    return Parse(document);
  }

  /// <summary>
  /// Parses an NBFC config from a stream of XML text.
  /// </summary>
  /// <param name="stream">The stream containing NBFC config XML.</param>
  /// <returns>The parsed <see cref="NbfcFanConfig"/>, or <see langword="null"/> if the content is not a usable config.</returns>
  public static NbfcFanConfig Parse(Stream stream) {
    if (stream == null)
      return null;

    XDocument document;
    try {
      document = XDocument.Load(stream);
    }
    catch (System.Xml.XmlException) {
      return null;
    }

    return Parse(document);
  }

  private static NbfcFanConfig Parse(XDocument document) {
    XElement root = document.Root;
    if (root == null)
      return null;

    // NBFC elements live in the default (empty) namespace; match by local name to be tolerant.
    var config = new NbfcFanConfig {
      NotebookModel = (GetElementValue(root, "NotebookModel") ?? string.Empty).Trim(),
      ReadWriteWords = ParseBool(GetElementValue(root, "ReadWriteWords")),
      ReadValueIsRpm = ParseBool(GetElementValue(root, "ReadValueIsRpm"))
    };

    XElement fanConfigurations = GetElement(root, "FanConfigurations");
    if (fanConfigurations != null) {
      foreach (XElement fan in GetElements(fanConfigurations, "FanConfiguration")) {
        config.Fans.Add(new NbfcFanConfiguration {
          ReadRegister = ParseInt(GetElementValue(fan, "ReadRegister")),
          MinSpeedValue = ParseInt(GetElementValue(fan, "MinSpeedValue")),
          MaxSpeedValue = ParseInt(GetElementValue(fan, "MaxSpeedValue")),
          IndependentReadMinMaxValues = ParseBool(GetElementValue(fan, "IndependentReadMinMaxValues")),
          MinSpeedValueRead = ParseInt(GetElementValue(fan, "MinSpeedValueRead")),
          MaxSpeedValueRead = ParseInt(GetElementValue(fan, "MaxSpeedValueRead")),
          FanDisplayName = (GetElementValue(fan, "FanDisplayName") ?? string.Empty).Trim()
        });
      }
    }

    return config;
  }

  private static XElement GetElement(XElement parent, string localName) {
    foreach (XElement e in parent.Elements()) {
      if (e.Name.LocalName == localName)
        return e;
    }

    return null;
  }

  private static System.Collections.Generic.IEnumerable<XElement> GetElements(XElement parent, string localName) {
    foreach (XElement e in parent.Elements()) {
      if (e.Name.LocalName == localName)
        yield return e;
    }
  }

  private static string GetElementValue(XElement parent, string localName) {
    return GetElement(parent, localName)?.Value;
  }

  private static int ParseInt(string value) {
    return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
  }

  private static bool ParseBool(string value) {
    return bool.TryParse(value, out bool result) && result;
  }
}
