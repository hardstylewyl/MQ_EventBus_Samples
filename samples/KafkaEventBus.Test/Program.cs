using EventBus.Extensions;
using EventBus.Kafka;
using Shared.Test;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var eventBusBuilder = builder.Services.AddKafkaEventBus(o =>
{
	o.BootstrapServers = "127.0.0.1:9192,127.0.0.1:9292,127.0.0.1:9392";

	o.ProducerTopic = "x-topic";
	o.ConsumerTopic = "x-topic";

	o.IsConsumer = true;
	o.ConsumerGroupId = "x-topic-g1";

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
