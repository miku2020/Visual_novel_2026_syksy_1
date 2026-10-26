using System.Collections;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

// Vertailee samaa laskentaa: pääsäie, Job, Burst Job ja GPU (compute shader).
// Lisää komponentti GameObjectiin, vedä BenchmarkCompute.compute kenttään, paina Play ja sitten Space
// (tai Context Menu -> "Run Benchmark"). Tulokset näkyvät Consolessa ja ruudulla.
public class PerformanceBenchmark : MonoBehaviour
{
    public ComputeShader computeShader;
    public int count = 1_000_000;
    public int iterations = 200;
    public int runsPerMethod = 3;

    string _report = "Paina Space ajaaksesi benchmarkin";
    bool _running;

    static float Work(float x, int iterations)
    {
        for (int k = 0; k < iterations; k++)
            x = Mathf.Sin(x) * Mathf.Cos(x) + Mathf.Sqrt(Mathf.Abs(x)) + 0.5f;
        return x;
    }

    // Ei Burst-käännetty: tavallinen Mono/IL2CPP-job.
    struct PlainJob : IJobParallelFor
    {
        public NativeArray<float> data;
        public int iterations;
        public void Execute(int i) => data[i] = Work(i * 0.001f, iterations);
    }

    [BurstCompile]
    struct BurstJob : IJobParallelFor
    {
        public NativeArray<float> data;
        public int iterations;
        public void Execute(int i)
        {
            float x = i * 0.001f;
            for (int k = 0; k < iterations; k++)
                x = math.sin(x) * math.cos(x) + math.sqrt(math.abs(x)) + 0.5f;
            data[i] = x;
        }
    }

    void Update()
    {
        if (!_running && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) RunBenchmark();
    }

    void OnGUI() => GUI.Label(new Rect(10, 10, 700, 300), _report);

    [ContextMenu("Run Benchmark")]
    public void RunBenchmark()
    {
        if (!_running) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        _running = true;
        _report = "Ajetaan...";
        yield return null;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Alkioita: {count:N0}, iteraatioita/alkio: {iterations}");

        var data = new NativeArray<float>(count, Allocator.TempJob);
        var managed = new float[count];

        // Pääsäie (rajoitettu, koska hidas)
        int mainCount = Mathf.Min(count, 100_000);
        double mainMs = Time(() =>
        {
            for (int i = 0; i < mainCount; i++) managed[i] = Work(i * 0.001f, iterations);
        });
        double mainScaled = mainMs * count / mainCount;
        sb.AppendLine($"Pääsäie (skaalattu {count:N0}:ään): {mainScaled:F1} ms");

        double plain = Best(() => new PlainJob { data = data, iterations = iterations }.Schedule(count, 64).Complete());
        sb.AppendLine($"Job (ei Burst):  {plain:F1} ms");

        // Ensimmäinen ajo kääntää Burstin, siksi lämmittely
        new BurstJob { data = data, iterations = iterations }.Schedule(count, 64).Complete();
        double burst = Best(() => new BurstJob { data = data, iterations = iterations }.Schedule(count, 64).Complete());
        sb.AppendLine($"Burst Job:       {burst:F1} ms  (CPU, checksum {data[count - 1]:F4})");
        data.Dispose();

        if (computeShader != null)
        {
            yield return GpuRun(sb);
        }
        else sb.AppendLine("GPU: computeShader puuttuu");

        sb.AppendLine($"Burst vs pääsäie: {mainScaled / burst:F1}x");
        _report = sb.ToString();
        Debug.Log(_report);
        _running = false;
    }

    IEnumerator GpuRun(System.Text.StringBuilder sb)
    {
        int kernel = computeShader.FindKernel("Bench");
        var buffer = new ComputeBuffer(count, sizeof(float));
        computeShader.SetBuffer(kernel, "Data", buffer);
        computeShader.SetInt("Iterations", iterations);
        computeShader.SetInt("Count", count);
        int groups = Mathf.CeilToInt(count / 256f);

        double best = double.MaxValue;
        float last = 0;
        for (int run = 0; run < runsPerMethod + 1; run++) // eka ajo lämmittelyä
        {
            var sw = Stopwatch.StartNew();
            computeShader.Dispatch(kernel, groups, 1, 1);
            var req = AsyncGPUReadback.Request(buffer);
            while (!req.done) yield return null;
            sw.Stop();
            if (req.hasError) { sb.AppendLine("GPU readback epäonnistui"); break; }
            last = req.GetData<float>()[count - 1];
            if (run > 0) best = System.Math.Min(best, sw.Elapsed.TotalMilliseconds);
        }
        buffer.Release();
        sb.AppendLine($"GPU compute:     {best:F1} ms  (sis. readback, ~frame-granulariteetti; checksum {last:F4})");
    }

    double Best(System.Action a)
    {
        double best = double.MaxValue;
        for (int i = 0; i < runsPerMethod; i++) best = System.Math.Min(best, Time(a));
        return best;
    }

    static double Time(System.Action a)
    {
        var sw = Stopwatch.StartNew();
        a();
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }
}
