export interface DeviceInfo {
  deviceId: string;
  deviceMetadata: string;
}

const BROWSERS = [
  { name: "Firefox", token: "Firefox" },
  { name: "Samsung Browser", token: "SamsungBrowser" },
  { name: "Opera", token: "Opera" },
  { name: "Opera", token: "OPR" },
  { name: "Edge", token: "Edge" },
  { name: "Chrome", token: "Chrome" },
  { name: "Safari", token: "Safari" },
];

const SYSTEMS = [
  { name: "Windows", token: "Windows" },
  { name: "macOS", token: "Macintosh" },
  { name: "Linux", token: "Linux" },
  { name: "Android", token: "Android" },
  { name: "iOS", token: "iPhone" },
  { name: "iOS", token: "iPad" },
];

export const getOrCreateDeviceIdentity = (): DeviceInfo => {
  let deviceId = localStorage.getItem("app_device_id");

  if (!deviceId) {
    deviceId = crypto.randomUUID();
    localStorage.setItem("app_device_id", deviceId);
  }

  const ua = navigator.userAgent;

  const browserName =
    BROWSERS.find((b) => ua.includes(b.token))?.name || "Unknown Browser";
  const os = SYSTEMS.find((s) => ua.includes(s.token))?.name || "Unknown OS";

  return {
    deviceId,
    deviceMetadata: `${browserName} on ${os}`,
  };
};
