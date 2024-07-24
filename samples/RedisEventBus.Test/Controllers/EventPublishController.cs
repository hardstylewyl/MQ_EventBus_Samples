using EventBus;
using Microsoft.AspNetCore.Mvc;

namespace RedisEventBus.Test.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventPublishController : ControllerBase
{
	private readonly IEventPublisher _eventPublisher;

	public EventPublishController(IEventPublisher eventPublisher)
	{
		_eventPublisher = eventPublisher;
	}


	[HttpGet]
	public async Task<IActionResult> Get()
	{
		await _eventPublisher.PublishAsync(new HelloEvent());
		return Ok();
	}

}
