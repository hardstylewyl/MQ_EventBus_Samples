using EventBus.Extensions;
using EventBus.Redis;
using Shared.Test;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var eventBusBuilder = builder.Services.AddRedisEventBus(config =>
{
	//"Host": "127.0.0.1:6379",
	//"Password": null,
	//"DbNumber": 0,
	//"IsConsumer": true,
	//"ConsumerTopic": "chat_event_bus",
	//"ProducerTopic": "chat_event_bus"

	config.Host = "127.0.0.1:6379";
	config.Password = null;
	config.DbNumber = 0;
	config.IsConsumer = true;
	config.ConsumerTopic = "hard_topic";
	config.ProducerTopic = "hard_topic";
});


eventBusBuilder.AddSubscription<HelloEvent, HelloEventHandler>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
