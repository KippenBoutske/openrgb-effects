using LibreHardwareMonitor.Hardware;
using OpenRgbEffects.Core;

namespace OpenRgbEffects.App.Services;

public sealed class TemperatureSensorService : IDisposable
{
    private readonly object _gate = new();
    private Computer? _computer;
    private double _cpu;
    private double _gpu;
    private bool _available;

    public void Start()
    {
        try
        {
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true
            };
            computer.Open();
            _computer = computer;
            Refresh();
        }
        catch (Exception)
        {
            _available = false;
        }
    }

    public void Refresh()
    {
        var computer = _computer;
        if (computer is null)
            return;

        try
        {
            double cpu = 0, gpu = 0;
            var cpuCount = 0;
            var gpuCount = 0;
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                Accumulate(hardware, ref cpu, ref cpuCount, ref gpu, ref gpuCount);
                foreach (var sub in hardware.SubHardware)
                {
                    sub.Update();
                    Accumulate(sub, ref cpu, ref cpuCount, ref gpu, ref gpuCount);
                }
            }

            lock (_gate)
            {
                _cpu = cpuCount > 0 ? cpu / cpuCount : 0;
                _gpu = gpuCount > 0 ? gpu / gpuCount : 0;
                _available = cpuCount > 0 || gpuCount > 0;
            }
        }
        catch (Exception)
        {
            lock (_gate)
                _available = false;
        }
    }

    public void CaptureInto(ref SensorSnapshot snapshot)
    {
        lock (_gate)
        {
            snapshot = snapshot with
            {
                HasTemperature = _available,
                CpuTempC = _cpu,
                GpuTempC = _gpu
            };
        }
    }

    public void Dispose()
    {
        try
        {
            _computer?.Close();
        }
        catch (Exception)
        {
        }
        _computer = null;
    }

    private static void Accumulate(IHardware hardware, ref double cpu, ref int cpuCount, ref double gpu, ref int gpuCount)
    {
        var isCpu = hardware.HardwareType is HardwareType.Cpu;
        var typeName = hardware.HardwareType.ToString();
        var isGpu = typeName.Contains("Gpu", StringComparison.OrdinalIgnoreCase);
        if (!isCpu && !isGpu)
            return;

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType != SensorType.Temperature || sensor.Value is not { } value)
                continue;
            if (sensor.Name.Contains("Distance", StringComparison.OrdinalIgnoreCase))
                continue;
            if (isCpu)
            {
                cpu += value;
                cpuCount++;
            }
            else
            {
                gpu += value;
                gpuCount++;
            }
        }
    }
}
