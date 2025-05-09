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
      cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
      {
         host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
         host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
      });

      cfg.ReceiveEndpoint("search-auction-created", e =>
      {
         e.UseMessageRetry(r => r.Interval(5, 5)); // 5 times with 5 seconds delta
         e.ConfigureConsumer<AucionCreatedConsumer>(context);
      });

      cfg.ReceiveEndpoint("search-auction-updated", e =>
      {
         e.UseMessageRetry(r => r.Interval(5, 5));
         e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
      });

      cfg.ReceiveEndpoint("search-auction-deleted", e =>
      {
         e.UseMessageRetry(r => r.Interval(5, 5));
         e.ConfigureConsumer<AuctionDeletedConsumer>(context);
      });

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