using Microsoft.AspNetCore.Mvc;
using NftIndexer.Entities;

namespace NftIndexer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NftController : ControllerBase
{

    private readonly ILogger<NftController> _logger;

    public NftController(ILogger<NftController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Token> Get()
    {
        return new List<Token>();
    }
}

