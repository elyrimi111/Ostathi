namespace Ostathi;

public class ImportResultDto
{
    public int AddedCount { get; set; }
    public int SkippedCount { get; set; }
    public bool Success => AddedCount > 0;
}
