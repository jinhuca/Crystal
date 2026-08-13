using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Crystal.Provider.Telemetry.Hardware.Motherboard.Lpc.EC.Nbfc;

/// <summary>
/// An index of NoteBook FanControl (NBFC) configs keyed by notebook model, used to look up the
/// config matching the current machine's SMBIOS system product name. NBFC selects a config by
/// exact model string; this store matches the same way but tolerant of case and surrounding
/// whitespace.
/// </summary>
internal sealed class NbfcConfigStore {
  private readonly Dictionary<string, NbfcFanConfig> _byModel = new();

  /// <summary>
  /// Initializes a store from a set of already-parsed configs. When two configs share a model
  /// (after normalization) the first one wins.
  /// </summary>
  /// <param name="configs">The configs to index.</param>
  public NbfcConfigStore(IEnumerable<NbfcFanConfig> configs) {
    if (configs == null)
      return;

    foreach (NbfcFanConfig config in configs) {
      if (config == null)
        continue;

      string key = Normalize(config.NotebookModel);
      if (key.Length != 0 && !_byModel.ContainsKey(key))
        _byModel.Add(key, config);
    }
  }

  /// <summary>
  /// Looks up the config for a machine model.
  /// </summary>
  /// <param name="productName">The SMBIOS system product name of the current machine.</param>
  /// <param name="config">The matching config, if any.</param>
  /// <returns><see langword="true"/> if a config was found for the model; otherwise <see langword="false"/>.</returns>
  public bool TryGetForModel(string productName, out NbfcFanConfig config) {
    config = null;
    string key = Normalize(productName);
    if (key.Length == 0)
      return false;

    return _byModel.TryGetValue(key, out config);
  }

  /// <summary>
  /// Looks up the config for a machine, trying several candidate model strings in order and
  /// returning the first match. Lenovo (and some others) report a machine-type code as the SMBIOS
  /// product name (e.g. "20MD..."), while NBFC configs key off the friendly name carried in the
  /// SMBIOS version field ("ThinkPad P1"); passing both lets either form match.
  /// </summary>
  /// <param name="candidates">Candidate model strings, tried in order.</param>
  /// <param name="config">The matching config, if any.</param>
  /// <returns><see langword="true"/> if a config matched any candidate; otherwise <see langword="false"/>.</returns>
  public bool TryGetForModel(IEnumerable<string> candidates, out NbfcFanConfig config) {
    config = null;
    if (candidates == null)
      return false;

    foreach (string candidate in candidates) {
      if (TryGetForModel(candidate, out config))
        return true;
    }

    config = null;
    return false;
  }

  /// <summary>
  /// Builds a store by parsing every <c>*.xml</c> file in a directory as an NBFC config.
  /// Returns an empty store when the directory does not exist.
  /// </summary>
  /// <param name="directory">The directory containing NBFC config files.</param>
  /// <returns>A populated (or empty) store.</returns>
  public static NbfcConfigStore FromDirectory(string directory) {
    var configs = new List<NbfcFanConfig>();

    if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) {
      foreach (string path in Directory.EnumerateFiles(directory, "*.xml", SearchOption.AllDirectories)) {
        NbfcFanConfig config = TryParseFile(path);
        if (config != null)
          configs.Add(config);
      }
    }

    return new NbfcConfigStore(configs);
  }

  private static NbfcFanConfig TryParseFile(string path) {
    try {
      using FileStream stream = File.OpenRead(path);
      return NbfcConfigParser.Parse(stream);
    }
    catch (IOException) {
      return null;
    }
    catch (System.UnauthorizedAccessException) {
      return null;
    }
  }

  // NBFC matches the model string exactly; normalize case and collapse whitespace so trivial
  // formatting differences between SMBIOS and the config file's <NotebookModel> still match.
  private static string Normalize(string value) {
    if (string.IsNullOrWhiteSpace(value))
      return string.Empty;

    string collapsed = Regex.Replace(value.Trim(), @"\s+", " ");
    return collapsed.ToLower(CultureInfo.InvariantCulture);
  }
}
