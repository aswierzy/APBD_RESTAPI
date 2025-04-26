using System.Text.Json;
using System.Text.Json.Nodes;
using DeviceManager.Entities;
using DeviceManager.Logic;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MY_DB");
builder.Services.AddSingleton<IDeviceService, DeviceService>(deviceService => new DeviceService(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/devices", (IDeviceService deviceService) =>
{
    try
    {
        return Results.Ok(deviceService.GetAll());
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.MapGet("/api/devices/{id}", (string id, IDeviceService deviceService) =>
{
    try
    {
        var device = deviceService.GetById(id);
        return device is not null
            ? Results.Ok(device)
            : Results.NotFound($"Device with ID '{id}' not found.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/api/devices", async (HttpRequest request, IDeviceService deviceService) =>
    {
        string? contentType = request.ContentType?.ToLower();

        switch (contentType)
        {
            case "application/json":
            {
                using var reader = new StreamReader(request.Body);
                string rawJson = await reader.ReadToEndAsync();

                var json = JsonNode.Parse(rawJson);

                if (json == null)
                    return Results.BadRequest("Invalid JSON.");

                var type = json["deviceType"];
                if (type == null)
                    return Results.BadRequest("Missing 'deviceType' property.");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                Device? device = type.ToString()?.ToLower() switch
                {
                    "personalcomputer" => JsonSerializer.Deserialize<PersonalComputer>(json["typeValue"].ToString(), options),
                    "smartwatch" => JsonSerializer.Deserialize<Smartwatch>(json["typeValue"].ToString(), options),
                    "embedded" => JsonSerializer.Deserialize<Embedded>(json["typeValue"].ToString(), options),
                    _ => null
                };

                if (device == null)
                    return Results.BadRequest("Invalid 'deviceType' provided.");

                var result = deviceService.Create(device);

                return result
                    ? Results.Created($"/api/devices/{device.Id}", device)
                    : Results.BadRequest("Failed to create device.");
            }

            case "text/plain":
                return Results.Ok();

            default:
                return Results.Conflict();
        }
    })
    .Accepts<string>("application/json", ["text/plain"]);



app.MapPut("/api/devices/{id}", async (HttpRequest request, string id, IDeviceService deviceService) =>
    {
        string? contentType = request.ContentType?.ToLower();

        switch (contentType)
        {
            case "application/json":
            {
                using var reader = new StreamReader(request.Body);
                string rawJson = await reader.ReadToEndAsync();

                var json = JsonNode.Parse(rawJson);

                if (json == null)
                    return Results.BadRequest("Invalid JSON.");

                var deviceType = json["deviceType"];
                if (deviceType == null)
                    return Results.BadRequest("Missing 'deviceType' field.");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                Device? device = deviceType.ToString()?.ToLower() switch
                {
                    "personalcomputer" => JsonSerializer.Deserialize<PersonalComputer>(json["typeValue"].ToString(), options),
                    "smartwatch" => JsonSerializer.Deserialize<Smartwatch>(json["typeValue"].ToString(), options),
                    "embedded" => JsonSerializer.Deserialize<Embedded>(json["typeValue"].ToString(), options),
                    _ => null
                };

                if (device == null)
                    return Results.BadRequest("Invalid 'deviceType' provided.");
                device.Id = id;

                var result = deviceService.Update(device);

                return result
                    ? Results.Ok($"Device with ID '{id}' updated successfully.")
                    : Results.NotFound($"Device with ID '{id}' not found.");
            }

            case "text/plain":
                return Results.Ok();
            default:
                return Results.Conflict();
        }
    })
    .Accepts<string>("application/json", ["text/plain"]);


app.MapDelete("/api/devices/{id}", (string id, IDeviceService deviceService) =>
{
    try
    {
        var result = deviceService.Delete(id);

        return result
            ? Results.Ok($"Device with ID '{id}' deleted successfully.")
            : Results.NotFound($"Device with ID '{id}' not found.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();