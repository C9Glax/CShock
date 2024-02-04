﻿using CShocker.Devices.Abstract;
using CShocker.Devices.Additional;
using CShocker.Devices.APIs;
using CShocker.Ranges;
using CShocker.Shockers;
using GlaxLogger;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

Logger logger = new (LogLevel.Trace);

Console.WriteLine("OpenShock API Key:");
string? apiKey = Console.ReadLine();
while(apiKey is null || apiKey.Length < 1)
    apiKey = Console.ReadLine();

/*
OpenShockHttp openShockHttp = new (new IntensityRange(30, 50), new DurationRange(1000, 1000), apiKey, logger: logger);
OpenShockShocker shocker = openShockHttp.GetShockers(apiKey).First();
shocker.Control(ControlAction.Vibrate, 20, 1000);

File.WriteAllText("shockers.json", JsonConvert.SerializeObject(shocker));
OpenShockShocker deserialized = JsonConvert.DeserializeObject<OpenShockShocker>(File.ReadAllText("shockers.json"), new ApiJsonConverter())!;
Thread.Sleep(1100); //Wait for previous to end
deserialized.Control(ControlAction.Vibrate, 20, 1000);
shocker.Dispose();
deserialized.Dispose();
*/


#pragma warning disable CA1416
List<SerialPortInfo> serialPorts = SerialHelper.GetSerialPorts();

if (serialPorts.Count < 1)
    return;

for(int i = 0; i < serialPorts.Count; i++)
    Console.WriteLine($"{i}) {serialPorts[i]}");

Console.WriteLine($"Select Serial Port [0-{serialPorts.Count-1}]:");
string? selectedPortStr = Console.ReadLine();
int selectedPort;
while (!int.TryParse(selectedPortStr, out selectedPort) || selectedPort < 0 || selectedPort > serialPorts.Count - 1)
{
    Console.WriteLine($"Select Serial Port [0-{serialPorts.Count-1}]:");
    selectedPortStr = Console.ReadLine();
}

OpenShockSerial openShockSerial = new(new IntensityRange(30, 50), new DurationRange(1000, 1000),serialPorts[selectedPort], apiKey, logger: logger);
OpenShockShocker shocker = openShockSerial.GetShockers().First();
shocker.Control(ControlAction.Vibrate, 20, 1000);
File.WriteAllText("shockers.json", JsonConvert.SerializeObject(shocker));
OpenShockShocker deserialized = JsonConvert.DeserializeObject<OpenShockShocker>(File.ReadAllText("shockers.json"), new ApiJsonConverter())!;
shocker.Dispose();
deserialized.Dispose();

logger.Dispose();