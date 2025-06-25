using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using EventCampusAPI.Models;
using EventCampusAPI.Validators;
using EventCampusAPI.Models.Response;
using EventCampusAPI.Models.Exceptions;
using System.IO;
using FluentValidation;

namespace EventCampusAPI.Middlewares
{

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Response'u capture etmek için memory stream kullan
            var originalBodyStream = context.Response.Body;
            
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Başarılı response'ları wrap et (sadece JSON response'lar için)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300 
                && context.Response.ContentType != null 
                && context.Response.ContentType.Contains("application/json"))
            {
                await HandleSuccessResponse(context, originalBodyStream);
            }
            else
            {
                // Diğer response'ları (static files, HTML vs.) olduğu gibi gönder
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception caught in middleware: {Message}", ex.Message);
            _logger.LogError("Exception Type: {ExceptionType}", ex.GetType().Name);
            _logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleSuccessResponse(HttpContext context, Stream originalBodyStream)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        // Eğer response boşsa veya zaten ApiResponse formatındaysa wrap etme
        if (string.IsNullOrEmpty(responseText) || responseText.Contains("\"IsSuccess\""))
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.Body.CopyToAsync(originalBodyStream);
            return;
        }

        // JSON parse et
        JsonDocument responseDoc = null;
        Dictionary<string, object> responseData = new Dictionary<string, object>();
        
        try
        {
            responseDoc = JsonDocument.Parse(responseText);
            // Response'daki tüm property'leri ana seviyeye taşı
            foreach (var property in responseDoc.RootElement.EnumerateObject())
            {
                responseData[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
            }
        }
        catch
        {
            // Parse edilemiyorsa string olarak kullan
            responseData["rawData"] = responseText;
        }

        // Ana response object'i oluştur
        var wrappedResponse = new Dictionary<string, object>
        {
            ["IsSuccess"] = true,
            ["StatusCode"] = context.Response.StatusCode,
            ["Message"] = GetSuccessMessage(context.Response.StatusCode),
            ["Errors"] = new List<string>()
        };

        // Response verilerini ana seviyeye ekle
        foreach (var kvp in responseData)
        {
            wrappedResponse[kvp.Key] = kvp.Value;
        }

        var jsonResponse = JsonSerializer.Serialize(wrappedResponse);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
        
        context.Response.ContentLength = bytes.Length;
        context.Response.Body = originalBodyStream;
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        
        responseDoc?.Dispose();
    }

    private static string GetSuccessMessage(int statusCode)
    {
        return statusCode switch
        {
            200 => "İşlem başarılı",
            201 => "Kayıt başarıyla oluşturuldu",
            204 => "İşlem başarıyla tamamlandı",
            _ => "İşlem başarılı"
        };
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Beklenmeyen bir hata oluştu";
        var errors = new List<string>();

        switch (exception)
        {
            case FluentValidation.ValidationException fluentValidationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Doğrulama hatası";
                errors.AddRange(fluentValidationEx.Errors.Select(e => e.ErrorMessage));
                break;

            case EventCampusAPI.Models.ValidationException validationEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Doğrulama hatası";
                errors.AddRange(validationEx.Errors.Select(e => e.ErrorMessage));
                break;

            case UnauthorizedAccessException _:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Yetkisiz erişim";
                errors.Add("Yetkisiz erişim");
                break;

            case KeyNotFoundException _:
                statusCode = (int)HttpStatusCode.NotFound;
                message = "Kaynak bulunamadı";
                errors.Add("Kaynak bulunamadı");
                break;

            case InvalidOperationException _:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = exception.Message;
                errors.Add(exception.Message);
                break;

            case EmailAlreadyExistsException _:
                statusCode = (int)HttpStatusCode.Conflict;
                message = "E-posta zaten kullanılıyor";
                errors.Add("E-posta zaten kullanılıyor");
                break;

            default:
                message = "Beklenmeyen bir hata oluştu";
                errors.Add($"Hata tipi: {exception.GetType().Name} - {exception.Message}");
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new Dictionary<string, object>
        {
            ["IsSuccess"] = false,
            ["StatusCode"] = statusCode,
            ["Message"] = message,
            ["Data"] = (object)null,
            ["Errors"] = errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
}