using System.ComponentModel.DataAnnotations;

namespace ImageApi.Models;

public sealed class Person
{
    public int Id { get; init; }
    
    [Required, MinLength(1)]
    public required string Name { get; init; }
    
    // .NET 9 なら DateOnly をネイティブで JSON バインド可（"yyyy-MM-dd" を送る）
    public DateOnly BirthDate { get; init; }
    
    [Required, MinLength(1)]
    public required string PartName { get; init; }
    
    // 画像の「保存名」（GUID拡張子）を格納
    [Required, MinLength(1)]
    public required string PhotoFileName { get; init; }
}