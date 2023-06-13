using Azure.Core;
using Azure;
using Image_gallery;
using Image_gallery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions { WebRootPath = "html" });

string connection = "Server=(localdb)\\mssqllocaldb;Database=applicationdb;Trusted_Connection=True;";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 33554432; // 32 ��
});
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/images", async (ApplicationContext db) => await db.Images.ToListAsync());

//app.MapGet("/", (ApplicationContext db) => db.Images.ToListAsync());
app.MapGet("/getimagesfromcategory", async (HttpContext context, ApplicationContext db) =>
{
    string category = context.Request.Query["category"];

    // ���������� ����������� �� ���� ������, ��������������� ��������� ���������
    List<Image> images = await db.Images.Where(image => image.Category == category).ToListAsync();

    // ������������� ������ ����������� � ������ URL-������� �����������
    List<string> imageUrls = images.Select(image => "uploads/" + image.FileName).ToList();

    // ������� ������ URL-������� ����������� � ���� JSON
    await context.Response.WriteAsJsonAsync(imageUrls);
});

app.MapDelete("/api/images", async (ApplicationContext db) =>
{
        // �������� ��� ������ �� ������� Images
        var allImages = db.Images.ToList();

        // ������� ��� ������ �� ������� Images
        db.Images.RemoveRange(allImages);

        // ��������� ��������� � ���� ������
        await db.SaveChangesAsync();
    
});

app.MapPost("/api/images", async (Image image, ApplicationContext db) =>
{
    await db.Images.AddAsync(image);
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

app.MapPost("/BufferedSingleFileUploadPhysical", async (HttpContext context) =>
{
    var file = context.Request.Form.Files[0];
    var uploadPath = $"{Directory.GetCurrentDirectory()}/wwwroot/uploads";
    Directory.CreateDirectory(uploadPath);
    string fullPath = Path.Combine(uploadPath, file.FileName);
    using (var fileStream = new FileStream(fullPath, FileMode.Create))
    {
        await file.CopyToAsync(fileStream);
    }
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";
});
/*
app.MapPost("/api/images", async (Image image, ApplicationContext db) =>
{
    // ��������� ������������ � ������
    await db.Images.AddAsync(image);
    await db.SaveChangesAsync();
    return image;
});
*/
app.MapPost("/add", async (context) =>
{
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";
    await response.SendFileAsync("wwwroot/html/add_new_files.html");
});

app.MapPost("/view", async (context) =>
{
    var response = context.Response;
    response.ContentType = "text/html; charset=utf-8";
    await response.SendFileAsync("wwwroot/html/view_images.html");
});

app.MapGet("/getimages", async (context) =>
{
    var response = context.Response;
    var uploadsPath = $"{Directory.GetCurrentDirectory()}/wwwroot/uploads";

    if (Directory.Exists(uploadsPath))
    {
        string[] imageFiles = Directory.GetFiles(uploadsPath);
        string[] fileNames = imageFiles.Select(file => "uploads/" + Path.GetFileName(file)).ToArray();
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteAsync(JsonSerializer.Serialize(fileNames));
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsync("Uploads directory not found");
    }
});




app.Run();
