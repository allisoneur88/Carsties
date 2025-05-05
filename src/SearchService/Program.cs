using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
   x.AddConsumersFromNamespaceContaining<AucionCreatedConsumer>();

   x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

   x.UsingRabbitMq((context, cfg) =>
   {
      cfg.ConfigureEndpoints(context);
   });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
   try
   {
      await DbInitializer.InitDb(app);
   }
   catch (Exception ex)
   {
      Console.WriteLine(ex);
   }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
   => HttpPolicyExtensions
      .HandleTransientHttpError()
      .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(2));