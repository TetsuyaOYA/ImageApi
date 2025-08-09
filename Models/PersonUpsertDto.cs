namespace ImageApi.Models;

public sealed class PersonUpsertDto
{
    public required string  Name          { get; init; }
    public required DateOnly BirthDate    { get; init; } // "yyyy-MM-dd" を送る
    public required string  PartName      { get; init; }
    public required string  PhotoFileName { get; init; }
}
