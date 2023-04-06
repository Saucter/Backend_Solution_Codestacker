using PDF_Reader_APIs.Shared;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("PDF/[controller]")]
public class ManagerPDF : ControllerBase
{
    protected readonly ManipulatorPDF manipulatorPDF = new ManipulatorPDF();
    protected readonly Database DB;
    public ManagerPDF(Database DB, ManipulatorPDF manipulatorPDF)
    {
        this.DB = DB;
        this.manipulatorPDF = manipulatorPDF;
        HttpContext.Request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("username" + ":" + "password")));
    }

    [HttpPost]
    public async Task<ActionResult<int>> PostPDF(List<IFormFile> Files)
    {
        foreach(var file in Files)
        {
        }

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