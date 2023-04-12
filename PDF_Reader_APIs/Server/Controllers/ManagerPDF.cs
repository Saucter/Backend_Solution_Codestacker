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
    public async Task<ActionResult<List<PDF>>> GetPDFs([FromQuery] List<int>? GetId)
    {
        return (GetId.Count() == 0) ? await DB.PDFs.Include(s => s.Sentences).ToListAsync() : await DB.PDFs.Where(x => GetId.Contains(x.id)).Include(s => s.Sentences).ToListAsync();
    }

    [HttpGet]
    public async Task<ActionResult<List<PDF>>> GetKeyword(int[]? id, string Keyword, bool? Exact)
    {
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetTopWords([FromQuery] List<int>? id, int? NumberOfWords, [FromQuery] List<string>? Ignore)
    {
        List<string> ListWords = (id.Count() == 0) ? ManipulatorPDF.GetWords(await DB.Sentences.ToListAsync()) : ManipulatorPDF.GetWords(await DB.PDFs.Where(x => id.Contains(x.id))
        .SelectMany(x => x.Sentences).ToListAsync());

        ListWords = ManipulatorPDF.RemoveStopWords(ListWords).Where(x => !string.IsNullOrEmpty(x)).ToList();
        var WordsGroup = ListWords.GroupBy(x => x).Where(x => x.Any());
        int MaxInGroup = WordsGroup.Max(x => x.Count());
        
        List<string> TopWords = new List<string>();
        int? NumOfWords = (NumberOfWords == null) ? 5 : NumberOfWords;
        int PreviousListWordsCount = 0;
        Ignore = Ignore.Select(x => x.ToLower()).ToList();
        
        for(int i = 0; i < NumOfWords; i++)
        {
            var Word = WordsGroup.Where(x => x.Count() == MaxInGroup).Select(x => x.Key).ToList();
            if(Word != null)
            {
                TopWords.AddRange(Word);
                WordsGroup = WordsGroup.Where(x => !Word.Contains(x.Key));
                for(int z = PreviousListWordsCount; z < PreviousListWordsCount + Word.Count(); z++)
                {
                    if(!Ignore.Contains(TopWords[z]) && TopWords[z].Length > 1 && TopWords[z].ToCharArray().Any(x => char.IsAsciiLetter(x)))
                    {
                        TopWords[z] = $"{i + 1}) {TopWords[z].Remove(1).ToUpper() + TopWords[z].Substring(1)} : Occurred {MaxInGroup} time(s)";
                    }
                    else
                    {
                        TopWords.Remove(TopWords[z]);
                        i--;
                    }
                }
                PreviousListWordsCount = TopWords.Count();
                try { MaxInGroup =  WordsGroup.Max(x => x.Count()); } catch {}
            }
        }
        return TopWords.OrderBy(x => x).ToList();
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
                DB.Remove(delete);
                try
                {
                    ResponseMessage = string.Concat(ResponseMessage, $"{x++}) Id: {delete.id} | Name: {delete.Name} | Blob file: deleted \n");
                    await AzureServices.DeleteFile("pdf-container", delete.FileLink);
                }
                catch 
                {
                    ResponseMessage = string.Concat(ResponseMessage, $"{x++}) Id: {delete.id} | Name: {delete.Name} | Blob file: Not found \n");
                }
            }
        }
        else{ return NotFound("No PDF(s) contain the submitted id(s)"); }
        await DB.SaveChangesAsync();
        return Ok(ResponseMessage);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAll()
    {
        List<PDF> ToBeDeleted = await DB.PDFs.ToListAsync();
        foreach(var delete in ToBeDeleted)
        {
            await AzureServices.DeleteFile("pdf-container", delete.FileLink);
        }
        DB.RemoveRange(ToBeDeleted);
        await DB.SaveChangesAsync();
        return Ok("All files were deleted");
    }
}