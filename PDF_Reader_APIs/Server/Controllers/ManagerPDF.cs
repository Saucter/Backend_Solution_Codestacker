using PDF_Reader_APIs.Shared.Entities;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("PDF/[controller]")]
public class pdfController : ControllerBase
{
    protected readonly ManipulatorPDF manipulatorPDF = new ManipulatorPDF();
    protected readonly Database DB;
    public pdfController(Database DB, ManipulatorPDF manipulatorPDF)
    {
        // IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings_Authentication.json", optional: false, reloadOnChange: false).Build();
        this.DB = DB;
        this.manipulatorPDF = manipulatorPDF;
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
            if(System.IO.Path.GetExtension(file.FileName) == "pdf")
            {
                PdfDocument FilePDF = manipulatorPDF.LoadPDF(file);
                ListPDF.Add(new PDF(file.FileName, file.Length, FilePDF.Pages.Count, manipulatorPDF.GetSentences(FilePDF)));
            }
            else
            {
                return BadRequest("Bad request: Only PDFs are accepted. File(s) sent is not a PDF.");           
            }
        }
        DB.Add(ListPDF);
        await DB.SaveChangesAsync();
        return await DB.PDFs.ToListAsync();
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