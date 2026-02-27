using System;

public class AiModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public decimal AccuracyScore { get; set; }
    public DateTime LastTrainedAt { get; set; }
}