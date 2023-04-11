using PDF_Reader_APIs.Shared.Entities;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;
using Microsoft.EntityFrameworkCore;
using PDF_Reader_APIs.Server.AzureStorageServices;

[ApiController]
[Route("PDF/[controller]/[action]")]
public class pdfController : ControllerBase
{
    protected readonly Database DB;
    protected readonly IAzureFileStorageService AzureServices;
    public pdfController(Database DB, IAzureFileStorageService AzureServices)
    {
        // IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings_Authentication.json", optional: false, reloadOnChange: false).Build();
        this.DB = DB;
        this.AzureServices = AzureServices;
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
                var FileInBytes = ManipulatorPDF.LoadBytePDF(file);
                PdfDocument FileLoader = ManipulatorPDF.LoadPDF(file);

                PDF FileInstance = new PDF(file.FileName, file.Length, FileLoader.Pages.Count, ManipulatorPDF.GetSentences(FileLoader), 
                await AzureServices.SaveFile(FileInBytes, file.FileName, "pdf-container"));

                DB.Add(FileInstance);
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
    public async Task<ActionResult<List<PDF>>> GetTopWords([FromQuery] List<int>? id, int NumberOfWords, [FromQuery] List<string>? Ignore)
    {
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult<List<PDF>>> DeletePDF([FromQuery] List<int> id)
    {
        List<PDF> ToBeDeleted = await DB.PDFs.Where(x => id.Contains(x.id)).ToListAsync();
        string ResponseMessage = "PDFs that were deleted: \n=======================\n";
        if(ToBeDeleted.Any())
        {
            int x = 1;
            foreach(var delete in ToBeDeleted)
            {
                ResponseMessage = string.Concat(ResponseMessage, $"{x++}) Id: {delete.id} | Name: {delete.Name}\n");
                DB.Remove(delete);
            }
        }
        else{ return NotFound("No PDF(s) contain the submitted id(s)"); }
        await DB.SaveChangesAsync();
        return Ok(ResponseMessage);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAll()
    {
        DB.RemoveRange(await DB.PDFs.ToListAsync());
        await DB.SaveChangesAsync();
        return Ok("All files were deleted");
    }
}