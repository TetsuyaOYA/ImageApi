using System.ComponentModel.DataAnnotations;

namespace ImageApi.Models;

/// <summary>アップロード画像のメタデータ</summary>
public class ImageFile
{
    public int Id { get; init; }

    [Required, MinLength(1)]
    public required string FileName { get; init; }

    [Required, MinLength(1)]
    public required string ContentType { get; init; }

    public long Length { get; init; }
    public DateTime UploadedAt { get; init; }
}