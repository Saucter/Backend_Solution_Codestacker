using PDF_Reader_APIs.Shared.Entities;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;
using Microsoft.EntityFrameworkCore;
using PDF_Reader_APIs.Server.AzureStorageServices;
using System.Linq;
using System.Text;
using PDF_Reader_APIs.Shared.ResponseTemplates;

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
    public async Task<ActionResult<List<PDF>>> GetKeyword([FromQuery] List<int>? id, string Keyword, bool? Exact, bool? CaseSensitive)
    {
        List<GetKeyWordResponse> Response = new List<GetKeyWordResponse>();
        List<PDF> ListPDF = (id.Count() == 0) ? await DB.PDFs.Include(s => s.Sentences).ToListAsync() : await DB.PDFs.Where(x => id.Contains(x.id)).Include(s => s.Sentences).ToListAsync(); 
        List<Sentences> ListSentences = new List<Sentences>();
        bool _Exact = Exact ?? false;
        bool _CaseSensitive = CaseSensitive ?? false;

        ListSentences.AddRange((_Exact == false) ? ListPDF.SelectMany(s => s.Sentences.Where(x => x.Sentence.Contains(Keyword))).ToList():
        ListPDF.SelectMany(s => s.Sentences.Where(x => x.Sentence.Split(new[] {' ', '-', '\'', '\"', ','}, StringSplitOptions.RemoveEmptyEntries).Contains(Keyword))).ToList());
        ListPDF = ListPDF.Where(x => ListSentences.Select(s => s.PDFid).Contains(x.id)).ToList();

        if(ListPDF.Count() != 0)
        {
            foreach(var pdf in ListPDF)
            {
                pdf.Sentences = ListSentences.Where(s => s.PDFid == pdf.id).ToList();
                Response.Add(new GetKeyWordResponse(pdf.id, pdf.Name, pdf.Sentences.Count(), pdf.Sentences.Select(x => x.Sentence).ToList(), Keyword, _Exact, _CaseSensitive));
            }
        }
        else
        {
            return NotFound(new StringBuilder().AppendFormat("The keyword '{0}' is not avilable in the submitted PDF(s)").ToString());
        }

        return Ok(Response);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetKeyWordResponse>>> GetTopWords([FromQuery] List<int>? id, int? NumberOfWords, [FromQuery] List<string>? Ignore)
    {
        List<GetTopWordsResponse> Response = new List<GetTopWordsResponse>();
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
                    if(!Ignore.Contains(TopWords[z]) && TopWords[z].Length > 1 && !TopWords[z].ToCharArray().Any(x => !char.IsAsciiLetter(x)))
                    {
                        TopWords[z] = TopWords[z].Remove(1).ToUpper() + TopWords[z].Substring(1);
                        Response.Add(new GetTopWordsResponse(TopWords[z], MaxInGroup, i+1));
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
        return Ok(Response.OrderBy(x => x.Position).ThenBy(x => x.Word).ToList());
    }

    [HttpDelete]
    public async Task<ActionResult> DeletePDF([FromQuery] List<int> id)
    {
        List<PDF> ToBeDeleted = await DB.PDFs.Where(x => id.Contains(x.id)).ToListAsync();
        StringBuilder ResponseMessage = new StringBuilder().Append("PDFs that were deleted: \n=======================\n");
        if(ToBeDeleted.Any())
        {
            int x = 1;
            foreach(var delete in ToBeDeleted)
            {
                DB.Remove(delete);
                try
                {
                    ResponseMessage = ResponseMessage.AppendFormat("{x++}) Id: {delete.id} | Name: {delete.Name} | Blob file: deleted \n", x++, delete.id, delete.Name);
                    await AzureServices.DeleteFile("pdf-container", delete.FileLink);
                }
                catch 
                {
                    ResponseMessage = ResponseMessage.AppendFormat("{x++}) Id: {delete.id} | Name: {delete.Name} | Blob file: Not found \n", x++, delete.id, delete.Name);
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