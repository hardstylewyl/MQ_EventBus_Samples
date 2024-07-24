using EventBus.Extensions;
using EventBus.RabbitMQ;
using Shared.Test;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRabbitMQEventBus(o =>
{
	//"ConnectionString": "amqp://localhost:5672",
	//   "QueueName": "chat.api",
	//   "ExchangeName": "chat_event_bus",
	//   "IsConsumer": true
	o.ConnectionString = "amqp://localhost:5672";
	o.QueueName = "x_queue";
	o.ExchangeName = "x_queue";
	o.IsConsumer = true;
})
	.AddSubscription<HelloEvent, HelloEventHandler>();



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
