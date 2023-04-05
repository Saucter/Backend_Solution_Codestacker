using Microsoft.AspNetCore.Mvc;
using PDF_Reader_APIs.Shared;

[ApiController]
[Route("PDF/[controller]")]
public class PDF_Manager : ControllerBase
{
    public PDF_Manager()
    {

    }

    [HttpPost]
    public async Task<ActionResult<int>> PostPDF()
    {
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<PDF>>> GetPDFs(int[]? id)
    {
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<PDF>>> GetKeyword(int[]? id, string Keyword, bool? Exact)
    {
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<PDF>>> GetTopWords(int[]? id, string? Ignore)
    {
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult<List<PDF>>> DeletePDF(int[] id)
    {
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult<List<PDF>>> DeleteAll()
    {
        return Ok();
    }
}