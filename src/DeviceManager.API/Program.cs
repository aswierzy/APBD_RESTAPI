using System.Text.Json;
using DeviceManager.Entities;
using DeviceManager.Logic;
using DeviceManager.Repository;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MY_DB")!;

builder.Services.AddScoped<IDeviceRepository>(_ => new DeviceRepository(connectionString));
builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/devices", (IDeviceService service) =>
{
    try
    {
        return Results.Ok(service.GetAll());
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.MapGet("/api/devices/{id}", (string id, IDeviceService service) =>
{
    try
    {
        var device = service.GetById(id);
        return device is not null ? Results.Ok(device) : Results.NotFound($"Device with ID '{id}' not found.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/api/devices", async (HttpRequest request, IDeviceService service) =>
{
    string? contentType = request.ContentType?.ToLower();

    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string rawJson = await reader.ReadToEndAsync();
            var json = JsonNode.Parse(rawJson);

            if (json == null) return Results.BadRequest("Invalid JSON.");
            var type = json["deviceType"];
            if (type == null) return Results.BadRequest("Missing 'deviceType' property.");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Device? device = type.ToString()?.ToLower() switch
            {
                "personalcomputer" => JsonSerializer.Deserialize<PersonalComputer>(json["typeValue"].ToString(),
                    options),
                "smartwatch" => JsonSerializer.Deserialize<Smartwatch>(json["typeValue"].ToString(), options),
                "embedded" => JsonSerializer.Deserialize<Embedded>(json["typeValue"].ToString(), options),
                _ => null
            };

            if (device == null) return Results.BadRequest("Invalid 'deviceType' provided.");

            try
            {
                var result = service.Create(device);
                return result
                    ? Results.Created($"/api/devices/{device.Id}", device)
                    : Results.BadRequest("Failed to create device.");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }
        case "text/plain":
            return Results.Ok();
        default:
            return Results.Conflict();
    }
});

app.MapPut("/api/devices/{id}", async (HttpRequest request, string id, IDeviceService service) =>
{
    string? contentType = request.ContentType?.ToLower();

    switch (contentType)
    {
        case "application/json":
        {
            using var reader = new StreamReader(request.Body);
            string rawJson = await reader.ReadToEndAsync();
            var json = JsonNode.Parse(rawJson);

            if (json == null) return Results.BadRequest("Invalid JSON.");
            var type = json["deviceType"];
            if (type == null) return Results.BadRequest("Missing 'deviceType' property.");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Device? device = type.ToString()?.ToLower() switch
            {
                "personalcomputer" => JsonSerializer.Deserialize<PersonalComputer>(json["typeValue"].ToString(), options),
                "smartwatch" => JsonSerializer.Deserialize<Smartwatch>(json["typeValue"].ToString(), options),
                "embedded" => JsonSerializer.Deserialize<Embedded>(json["typeValue"].ToString(), options),
                _ => null
            };

            if (device == null) return Results.BadRequest("Invalid 'deviceType' provided.");
            device.Id = id;

            try
            {
                var result = service.Update(device);
                return result ? Results.Ok($"Device with ID '{id}' updated successfully.") : Results.NotFound($"Device with ID '{id}' not found.");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }
        case "text/plain":
            return Results.Ok();
        default:
            return Results.Conflict();
    }
}).Accepts<string>("application/json", ["text/plain"]);

app.MapDelete("/api/devices/{id}", (string id, string rowVersionBase64, IDeviceService service) =>
{
    try
    {
        var rowVersion = Convert.FromBase64String(rowVersionBase64);
        var result = service.Delete(id, rowVersion);
        
        return result
            ? Results.Ok($"Device with ID '{id}' deleted successfully.")
            : Results.NotFound($"Device with ID '{id}' not found or concurrency conflict occurred.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
