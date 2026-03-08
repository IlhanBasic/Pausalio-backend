using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pausalio.Functions;
using System.Net;
using System.Net.Mail;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["DefaultConnection"];

        services.AddDbContext<PausalioFunctionsDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

        services.AddScoped<SmtpClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            return new SmtpClient(config["SmtpHost"], int.Parse(config["SmtpPort"]!))
            {
                Credentials = new NetworkCredential(config["SmtpUser"], config["SmtpPass"]),
                EnableSsl = true
            };
        });
    })
    .Build();

host.Run();