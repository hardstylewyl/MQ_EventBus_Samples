using EventBus.Extensions;
using EventBus.RocketMQ;
using Shared.Test;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var eventBusBuilder = builder.Services.AddRocketMQEventBus(o =>
{
	//	  "Endpoints": "127.0.0.1:9876",
	//   "ConsumerTopic": "chat_event_bus",
	//   "ConsumerGroup": "chat.api",
	//	  "ProducerTopic": "chat_event_bus",
	//   "ProducerGroup": "chat.longConnectionServer"
	//   "IsConsumer": true

	o.Endpoints = "127.0.0.1:9876";
	o.ConsumerTopic = "x-topic";
	o.ConsumerGroup = "x-topic-cg";

	o.ProducerTopic = "x-topic";
	o.ProducerGroup = "x-topic-pg";
	o.IsConsumer = true;
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
