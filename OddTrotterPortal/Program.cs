using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

var key = Guid.Parse("").ToByteArray();
var iv = Guid.Parse("").ToByteArray();
OddTrotter.OddTrotter.Decrypt(key, iv);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddHttpContextAccessor();

builder.WebHost.UseUrls("https://*:443");

var app = builder.Build();

app.UseHttpsRedirection();
app.MapFallbackToPage("/_Host");

app.Run();
