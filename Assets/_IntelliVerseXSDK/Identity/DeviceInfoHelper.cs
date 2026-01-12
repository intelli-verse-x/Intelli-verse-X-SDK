using System;
using UnityEngine;
#if !UNITY_WEBGL
using System.Linq;
using System.Net.NetworkInformation; // Not available in WebGL
#endif

public static class DeviceInfoHelper
{
    private const string PseudoIdKey = "DeviceInfoHelper_PseudoId";

    /// <summary>
    /// Returns the required login fields: fromDevice ("webgl" or "machine") and macAddress (real or stable pseudo).
    /// </summary>
    public static void GetLoginDeviceFields(out string fromDevice, out string macAddress)
    {
#if UNITY_WEBGL
        fromDevice = "webgl";
        // WebGL cannot access hardware identifiers; use a stable pseudo-id
        macAddress = GetOrCreatePseudoId();
#else
        fromDevice = "machine";
        // Try real MAC first (desktop). On mobile/console this will typically return empty -> fallback.
        macAddress = TryGetPrimaryMacAddress();

        if (string.IsNullOrWhiteSpace(macAddress))
            macAddress = GetOrCreatePseudoId();
#endif
    }

#if !UNITY_WEBGL
    /// <summary>
    /// Best-effort MAC retrieval for desktop (Windows/macOS/Linux). Returns empty string if unavailable.
    /// </summary>
    private static string TryGetPrimaryMacAddress()
    {
        try
        {
            // Filter to physical, up, non-loopback adapters with a valid MAC
            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Select(nic => nic.GetPhysicalAddress()?.ToString())
                .Where(addr => !string.IsNullOrWhiteSpace(addr));

            // Pick the first non-empty MAC and format as 00:11:22:33:44:55
            string raw = nics.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            return FormatMac(raw);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FormatMac(string raw)
    {
        // Raw is usually "001122334455"
        if (raw.Length < 12) return raw;
        return string.Join(":", Enumerable.Range(0, 6).Select(i => raw.Substring(i * 2, 2)));
    }
#endif

    /// <summary>
    /// Creates or returns a stable ID when MAC isn't accessible (WebGL, mobile, privacy restrictions).
    /// Stored in PlayerPrefs.
    /// </summary>
    private static string GetOrCreatePseudoId()
    {
        string id = PlayerPrefs.GetString(PseudoIdKey, string.Empty);
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N"); // 32-char hex
            PlayerPrefs.SetString(PseudoIdKey, id);
            PlayerPrefs.Save();
        }
        return id;
    }
}
