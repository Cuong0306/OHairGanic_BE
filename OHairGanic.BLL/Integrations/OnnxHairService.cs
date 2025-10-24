// Sửa nhẹ OnnxHairService: thêm thuộc tính ModelVersion, không tự lưu DB
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Responses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

public class OnnxHairService : IHairAnalysisService
{
    private readonly InferenceSession _session;
    private readonly string[] _labels;

    public string ModelVersion { get; } = "hair.onnx@v1"; // hoặc load từ metadata nếu có

    public OnnxHairService()
    {
        var modelPath = Path.Combine(AppContext.BaseDirectory, "Models", "Hair", "hair.onnx");
        var labelsPath = Path.Combine(AppContext.BaseDirectory, "Models", "Hair", "labels.txt");
        if (!File.Exists(modelPath)) throw new FileNotFoundException("Không tìm thấy model ONNX!", modelPath);
        if (!File.Exists(labelsPath)) throw new FileNotFoundException("Không tìm thấy file labels.txt!", labelsPath);
        _session = new InferenceSession(modelPath);
        _labels = File.ReadAllLines(labelsPath);
    }

    public async Task<HairAnalyzeResponse> AnalyzeImageAsync(Stream imageStream) => await ProcessImageAsync(imageStream);

    public async Task<HairAnalyzeResponse> AnalyzeFromUrlAsync(string imageUrl, string angle)
    {
        using var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync(imageUrl);
        using var stream = new MemoryStream(bytes);
        return await ProcessImageAsync(stream);
    }

    private async Task<HairAnalyzeResponse> ProcessImageAsync(Stream stream)
    {
        using var image = await Image.LoadAsync<Rgba32>(stream);
        image.Mutate(x => x.Resize(224, 224));

        var input = new DenseTensor<float>(new[] { 1, 224, 224, 3 });
        for (int y = 0; y < 224; y++)
            for (int x = 0; x < 224; x++)
            {
                var p = image[x, y];
                input[0, y, x, 0] = p.R / 255f;
                input[0, y, x, 1] = p.G / 255f;
                input[0, y, x, 2] = p.B / 255f;
            }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), input)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        int predictedIndex = Array.IndexOf(output, output.Max());
        var label = _labels[predictedIndex];

        return new HairAnalyzeResponse
        {
            Label = label,
            Oiliness = output.Length > 0 ? output[0] * 100 : 0,
            Dryness = output.Length > 1 ? output[1] * 100 : 0,
            DandruffScore = output.Length > 2 ? output[2] * 100 : 0
        };
    }
}
