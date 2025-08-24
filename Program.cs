using Microsoft.EntityFrameworkCore;
using ImageApi.Data;
using ImageApi.Models;
using Microsoft.AspNetCore.Http.Features;   // FormOptions
using Microsoft.AspNetCore.StaticFiles;     // FileExtensionContentTypeProvider
using Microsoft.AspNetCore.Antiforgery;     // .DisableAntiforgery()

var builder = WebApplication.CreateBuilder(args);

// ---- Services
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=images.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// アップロード全体のサイズ上限（例：10 MB）
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// ---- Middleware
// app.UseHttpsRedirection();  // 必要なら有効化
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---- Paths (絶対パスで統一)
var uploadDir = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadDir);

// ---- 共有ユーティリティ
var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{ ".png", ".jpg", ".jpeg", ".gif", ".webp" };

var contentTypeProvider = new FileExtensionContentTypeProvider();

// ---- Endpoints

// POST /api/images  ─ アップロード
app.MapPost("/api/images", async (IFormFile file, AppDbContext db) =>
{
    if (file.Length == 0) return Results.BadRequest("empty file");

    var ext = Path.GetExtension(file.FileName);
    if (!allowedExt.Contains(ext))
        return Results.BadRequest("unsupported extension");

    // 拡張子から Content-Type を推定（失敗時は octet-stream）
    if (!contentTypeProvider.TryGetContentType("x" + ext, out var contentType))
        contentType = "application/octet-stream";

    var storedFileName = $"{Guid.NewGuid():N}{ext}";
    var storedPath = Path.Combine(uploadDir, storedFileName);

    try
    {
        // 保存
        await using (var fs = System.IO.File.Create(storedPath))
            await file.CopyToAsync(fs);

        // DB 追加
        var entity = new ImageFile
        {
            FileName = storedFileName,
            ContentType = contentType,
            Length = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        db.Images.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/images/{entity.Id}",
            new { entity.Id, DownloadUrl = $"/api/images/{entity.Id}/content" });
    }
    catch
    {
        // 失敗時はファイルをロールバック
        if (System.IO.File.Exists(storedPath))
            System.IO.File.Delete(storedPath);
        throw;
    }
})
.DisableAntiforgery(); // Swagger からの POST テスト時に CSRF 不要

// GET /api/images/{id}  ─ メタデータ
app.MapGet("/api/images/{id:int}", async (int id, AppDbContext db)
        => await db.Images.FindAsync(id) is { } img
            ? Results.Ok(img)
            : Results.NotFound())
   .Produces<ImageFile>(StatusCodes.Status200OK, "application/json")
   .Produces(StatusCodes.Status404NotFound);

// GET /api/images/{id}/content  ─ バイナリ取得
app.MapGet("/api/images/{id:int}/content", async (int id, AppDbContext db) =>
{
    var img = await db.Images.FindAsync(id);
    if (img is null) return Results.NotFound();

    var physical = Path.Combine(uploadDir, img.FileName);
    if (!System.IO.File.Exists(physical))
        return Results.StatusCode(StatusCodes.Status410Gone);

    // 絶対パスで返す（Range 対応）
    return Results.File(
        physical,
        contentType: img.ContentType,
        fileDownloadName: img.FileName,
        enableRangeProcessing: true);
})
.Produces<byte[]>(StatusCodes.Status200OK, "application/octet-stream")
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status410Gone);

// POST /api/persons  ─ Person 作成
app.MapPost("/api/persons", async (PersonUpsertDto dto, AppDbContext db) =>
    {
        var entity = new Person
        {
            Name = dto.Name,
            PartName = dto.PartName,
            PhotoFileName = dto.PhotoFileName,
            BirthDate = dto.BirthDate
        };

        db.Persons.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/api/persons/{entity.Id}", entity);
    })
    .Produces<Person>(StatusCodes.Status201Created)
    .ProducesValidationProblem();

// GET /api/persons/{id} ─ 取得
app.MapGet("/api/persons/{id:int}", async (int id, AppDbContext db) =>
        await db.Persons.FindAsync(id) is { } p ? Results.Ok(p) : Results.NotFound())
    .Produces<Person>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// 動作確認用
app.MapGet("/ping", () => "pong");
app.MapGet("/ping_x", () => "pong");

app.Run();
//
