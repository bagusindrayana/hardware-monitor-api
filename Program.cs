using System.Net;
using System.Text.Json;
using LibreHardwareMonitor.Hardware;


public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

class Program
{
    static void Main()
    {
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true,
            IsNetworkEnabled = true
        };
        computer.Open();
        // computer.Accept(new UpdateVisitor());

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8085/");
        listener.Start();
        Console.WriteLine("HTTP server running at http://localhost:8085/data.json");

        while (true)
        {
            var context = listener.GetContext();
            if (context.Request.Url?.AbsolutePath == "/data.json")
            {
                var readings = GetSensorData(computer);
                string json = JsonSerializer.Serialize(readings, new JsonSerializerOptions { WriteIndented = true });

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }
    }

    static List<object> GetSensorData(Computer computer)
    {
        var list = new List<object>();
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (IHardware subhardware in hardware.SubHardware)
            {
                
                foreach (ISensor sensor in subhardware.Sensors)
                {
                    list.Add(new
                    {
                        Hardware = hardware.Name,
                        HardwareType =  Enum.GetName(typeof(HardwareType), hardware.HardwareType),
                        Sensor = sensor.Name,
                        Type = sensor.SensorType.ToString(),
                        Value = sensor.Value ?? 0
                    });
                }
            }
            foreach (ISensor sensor in hardware.Sensors)
            {
                list.Add(new
                {
                    Hardware = hardware.Name,
                    HardwareType = Enum.GetName(typeof(HardwareType), hardware.HardwareType),
                    Sensor = sensor.Name,
                    Type = sensor.SensorType.ToString(),
                    Value = sensor.Value ?? 0
                });
            }
        }
        return list;
    }
}


// using System;
// using System.Collections.Generic;
// using System.Text.Json;
// using LibreHardwareMonitor.Hardware;

// public class UpdateVisitor : IVisitor
// {
//     public void VisitComputer(IComputer computer) => computer.Traverse(this);

//     public void VisitHardware(IHardware hardware)
//     {
//         hardware.Update();
//         foreach (IHardware subHardware in hardware.SubHardware)
//             subHardware.Accept(this);
//     }

//     public void VisitSensor(ISensor sensor) { }
//     public void VisitParameter(IParameter parameter) { }
// }

// class Program
// {
//     static void Main()
//     {
//         var computer = new Computer
//         {
//             IsCpuEnabled = true,
//             IsGpuEnabled = true,
//             IsMotherboardEnabled = true,
//             IsMemoryEnabled = true,
//             IsStorageEnabled = true,
//             IsNetworkEnabled = true
//         };
//         computer.Open();
//         computer.Accept(new UpdateVisitor());

//         var readings = GetSensorData(computer);

//         string json = JsonSerializer.Serialize(readings, new JsonSerializerOptions
//         {
//             WriteIndented = true
//         });

//         Console.WriteLine(json); // Output JSON to stdout
//     }

//     static List<object> GetSensorData(Computer computer)
//     {
//         var list = new List<object>();

//         foreach (IHardware hardware in computer.Hardware)
//         {
//             foreach (IHardware subhardware in hardware.SubHardware)
//             {
//                 foreach (ISensor sensor in subhardware.Sensors)
//                 {
//                     list.Add(new
//                     {
//                         Hardware = hardware.Name,
//                         Sensor = sensor.Name,
//                         Type = sensor.SensorType.ToString(),
//                         Value = sensor.Value ?? 0
//                     });
//                 }
//             }

//             foreach (ISensor sensor in hardware.Sensors)
//             {
//                 list.Add(new
//                 {
//                     Hardware = hardware.Name,
//                     Sensor = sensor.Name,
//                     Type = sensor.SensorType.ToString(),
//                     Value = sensor.Value ?? 0
//                 });
//             }
//         }

//         return list;
//     }
// }
