using DeviceManager.Entities;
using DeviceManager.Logic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var deviceService = new DeviceService();

app.MapGet("/devices", () => Results.Ok(deviceService.GetAll()));

app.MapGet("/devices/{id}", (string id) =>
{
    var device = deviceService.GetById(id);
    return device is not null ? Results.Ok(device) : Results.NotFound($"No device with id={id}");
});

app.MapPost("/devices/computers", (PersonalComputer pc) =>
{
    try
    {
        deviceService.Create(pc);
        return Results.Created($"/devices/{pc.Id}", pc);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/devices/smartwatches", (Smartwatch watch) =>
{
    try
    {
        deviceService.Create(watch);
        return Results.Created($"/devices/{watch.Id}", watch);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/devices/embedded", (Embedded embedded) =>
{
    try
    {
        deviceService.Create(embedded);
        return Results.Created($"/devices/{embedded.Id}", embedded);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/devices/computers/{id}", (string id, PersonalComputer updatedPc) =>
{
    try
    {
        var existing = deviceService.GetById(id);
        if (existing is null)
        {
            return Results.NotFound($"No device with id={id} found.");
        }

        if (existing is not PersonalComputer)
        {
            return Results.BadRequest($"Device {id} is not a PersonalComputer; it is {existing.GetType().Name}.");
        }
        deviceService.Update(id, updatedPc);
        return Results.Ok(updatedPc);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/devices/smartwatches/{id}", (string id, Smartwatch updatedWatch) =>
{
    try
    {
        var existing = deviceService.GetById(id);
        if (existing is null)
        {
            return Results.NotFound($"No device with id={id} found.");
        }

        if (existing is not Smartwatch)
        {
            return Results.BadRequest($"Device {id} is not a Smartwatch; it is {existing.GetType().Name}.");
        }
        deviceService.Update(id, updatedWatch);
        return Results.Ok(updatedWatch);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/devices/embedded/{id}", (string id, Embedded updatedEmbedded) =>
{
    try
    {
        var existing = deviceService.GetById(id);
        if (existing is null)
        {
            return Results.NotFound($"No device with id={id} found.");
        }

        if (existing is not Embedded)
        {
            return Results.BadRequest($"Device {id} is not an Embedded device; it is {existing.GetType().Name}.");
        }

        deviceService.Update(id, updatedEmbedded);

        return Results.Ok(updatedEmbedded);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapDelete("/devices/{id}", (string id) =>
{
    try
    {
        deviceService.Delete(id);
        return Results.Ok($"Device with id={id} deleted.");
    }
    catch (Exception ex)
    {
        return ex.Message.Contains("not found")
            ? Results.NotFound(ex.Message)
            : Results.BadRequest(ex.Message);
    }
});

app.Run();