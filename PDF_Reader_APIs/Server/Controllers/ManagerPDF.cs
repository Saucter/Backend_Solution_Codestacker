using PDF_Reader_APIs.Shared.Entities;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("PDF/[controller]/[action]")]
public class pdfController : ControllerBase
{
    protected readonly ManipulatorPDF manipulatorPDF = new ManipulatorPDF();
    protected readonly Database DB;
    public pdfController(Database DB)
    {
        // IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings_Authentication.json", optional: false, reloadOnChange: false).Build();
        this.DB = DB;
        // string username = config.GetSection("AuthenticationHeader")["username"];
        // string password = config.GetSection("AuthenticationHeader")["password"];
        // HttpContext.Request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}")));
    }

    [HttpPost]
    public async Task<ActionResult<List<PDF>>> PostPDF(List<IFormFile> Files)
    {
        List<PDF> ListPDF = new List<PDF>();
        foreach(var file in Files)
        {
            if(System.IO.Path.GetExtension(file.FileName) == ".pdf")
            {
                PdfDocument FileLoader = manipulatorPDF.LoadPDF(file);
                PDF FileInstance = new PDF(file.FileName, file.Length, FileLoader.Pages.Count, manipulatorPDF.GetSentences(FileLoader), "Dummy string for testing");
                DB.Add(FileInstance);
                // FileInstance.Sentences = null;
                ListPDF.Add(FileInstance);
            }
            else
            {
                return BadRequest("Bad request: Only PDFs are accepted. File(s) sent is not a PDF");           
            }
        }
        await DB.SaveChangesAsync();
        return ListPDF;
    }

    [HttpGet]
    public async Task<ActionResult<List<PDF>>> GetPDFs([FromQuery] List<int> GetId)
    {
        return (GetId == null) ? await DB.PDFs.Include(s => s.Sentences).ToListAsync() : await DB.PDFs.Where(x => GetId.Contains(x.id)).Include(s => s.Sentences).ToListAsync();
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