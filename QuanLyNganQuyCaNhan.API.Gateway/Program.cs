using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("QuanLyNganQuyCaNhan.Ocelot.json", optional: false, reloadOnChange: true)
    // Cho phép cấu hình địa chỉ downstream bằng biến môi trường khi triển khai.
    .AddEnvironmentVariables();
if (args.Length > 0) builder.Configuration.AddCommandLine(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // API import cho phép request 6 MB, trong đó file tối đa 5 MB.
    options.Limits.MaxRequestBodySize = 6 * 1024 * 1024;
});
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Ocelot dùng middleware, không dùng MapGet/MapControllers tại Gateway.
app.Use(async (context, next) =>
{
    if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        && context.Request.Path == "/")
    {
        context.Response.Redirect("/QuanLyNganQuyCaNhan/QuanLyNganQuyCaNhan.html");
        return;
    }

    if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/health/live")
    {
        await context.Response.WriteAsJsonAsync(new { TrangThai = "Healthy", DichVu = "QuanLyNganQuyCaNhan.API.Gateway" });
        return;
    }

    // Bỏ IP do client tự gửi; Ocelot thêm IP kết nối thật qua UpstreamHeaderTransform.
    context.Request.Headers.Remove("X-Forwarded-For");
    await next(context);
});

await app.UseOcelot();
await app.RunAsync();
