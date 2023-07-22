using System.Net;
using Microsoft.AspNetCore.Mvc;
using NftIndexer.Entities;
using NftIndexer.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace NftIndexer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NftController : ControllerBase
{

    private readonly ILogger<NftController> _logger;
    private readonly ITokenService _tokenService;
    private string _ipfsGateway;

    public NftController(ILogger<NftController> logger, ITokenService tokenService, IConfiguration configuration)
    {
        _logger = logger;
        _tokenService = tokenService;
        _ipfsGateway = configuration["IpfsGateway"];
    }

    [HttpGet]
    public async Task<List<Token>> Get()
    {
        return await _tokenService.GetToken();
    }

    //[HttpGet("GetImage")]
    //public IActionResult GetImage(string uri)
    //{
    //    string url = uri.Replace("ipfs://", _ipfsGateway);

    //    WebClient client = new WebClient();
    //    byte[] imageData = client.DownloadData(url);
    //    Stream stream = client.OpenRead(url);
    //    string base64String = Convert.ToBase64String(imageData);

    //    return File(stream, "image/png", "image.png");
    //}
}

