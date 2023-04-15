using PDF_Reader_APIs.Shared.Entities;
using PDF_Reader_APIs.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Spire.Pdf;
using Microsoft.EntityFrameworkCore;
using PDF_Reader_APIs.Server.AzureStorageServices;
using PDF_Reader_APIs.Server.Authentication;
using System.Linq;
using System.Text;
using PDF_Reader_APIs.Shared.ResponseTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("PDF/[controller]/[action]")]
[Authorize(AuthenticationSchemes = "BasicAuthentication")]
public class pdfController : ControllerBase
{
    protected readonly Database DB;
    protected readonly IAzureFileStorageService AzureServices;
    protected readonly ICacheRepository Cache;
    protected readonly IUserRepository UserAuthenticator;
    public pdfController(Database DB, IAzureFileStorageService AzureServices, IUserRepository UserAuthenticator, ICacheRepository Cache)
    {
        this.DB = DB;
        this.AzureServices = AzureServices;
        this.UserAuthenticator = UserAuthenticator;
        this.Cache = Cache;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<List<PDF>>> PostPDF(List<IFormFile> Files)
    {
        List<PDF> ListPDF = new List<PDF>();
        foreach(var file in Files)
        {
            if(System.IO.Path.GetExtension(file.FileName) == ".pdf")
            {
                var FileInBytes = ManipulatorPDF.LoadBytePDF(file);
                PdfDocument FileLoader = ManipulatorPDF.LoadPDF(file);
                List<Sentences> Sentences = ManipulatorPDF.GetSentences(FileLoader);

                PDF FileInstance = new PDF(file.FileName, file.Length, FileLoader.Pages.Count, Sentences, await AzureServices.SaveFile(FileInBytes, file.FileName, "pdf-container"),
                await AzureServices.SaveFile(ManipulatorPDF.SentencesToText(Sentences), file.FileName.Substring(0, file.FileName.Length - 4)+"_Sentence.txt", "sentences-container"));

                DB.Add(FileInstance);
                ListPDF.Add(FileInstance);
            }
            else
            {
                return BadRequest("Bad request: Only PDFs are accepted. File(s) sent is not a PDF");       
            }
        }
        Cache.Remove("ListPDF");
        await DB.SaveChangesAsync();
        return ListPDF;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<GetPdfResponse>>> GetPDFs([FromQuery] List<int>? GetId, bool WithSentences)
    {
        List<PDF> ListPDFs = await Cache.GetCache("ListPDF", GetId);
        List<GetPdfResponse> Response = new List<GetPdfResponse>();
        if(ListPDFs.Any(x => GetId.Contains(x.id)) || GetId.Count() == 0)
        {
            List<PDF> ToBeReturned = (GetId.Count() == 0) ? ListPDFs : ListPDFs.Where(x => GetId.Contains(x.id)).ToList();
            if(WithSentences)
            {
                return Ok(ToBeReturned);
            }
            else
            {
                foreach(var pdf in ToBeReturned)
                {
                    Response.Add(new GetPdfResponse(pdf.id, pdf.Name, pdf.FileSize, pdf.NumberOfPages, pdf.FileLink, pdf.SentencesLinkTxt));
                }
                return Ok(Response);
            }
            
        } 
        else
        {
            return NotFound("The PDF with submitted ID(s) is unavailable");
        }       
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<GetKeyWordResponse>>> GetKeyword([FromQuery] List<int>? id, string Keyword, bool? Exact, bool? CaseSensitive)
    {
        List<GetKeyWordResponse> Response = new List<GetKeyWordResponse>();
        List<PDF> ListPDF = await Cache.GetCache("ListPDF", id);
        List<Sentences> ListSentences = new List<Sentences>();
        bool _Exact = Exact ?? false;
        bool _CaseSensitive = CaseSensitive ?? false;

        if(!_CaseSensitive)
        {
            Keyword = Keyword.ToLower();
            ListPDF.Select(pdf => 
            {
                pdf.Sentences.ForEach(s => s.Sentence = s.Sentence.ToLower());
                return pdf;
            });
        }   

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
            return NotFound(new StringBuilder().AppendFormat("The keyword '{0}' is not avilable in the submitted PDF(s)", Keyword).ToString());
        }

        int Total = 0;
        Response.ForEach(x => Total += x.Occurrences);
        Response.ForEach(x => x.TotalOccurrences = Total);
        
        return Ok(Response);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<GetKeyWordResponse>>> GetTopWords([FromQuery] List<int>? id, int? NumberOfWords, [FromQuery] List<string>? Ignore)
    {
        List<GetTopWordsResponse> Response = new List<GetTopWordsResponse>();
        List<PDF> ListPDF = await Cache.GetCache("ListPDF", id);
        List<string> ListWords = ManipulatorPDF.GetWords(ListPDF.SelectMany(x => x.Sentences).ToList());

        ListWords = ManipulatorPDF.RemoveStopWords(ListWords).Where(x => !string.IsNullOrEmpty(x)).ToList();
        var WordsGroup = ListWords.GroupBy(x => x).Where(x => x.Key.Length > 1 && x.Key.ToCharArray().All(k => char.IsLetter(k)));
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
                        Word.Remove(TopWords[z]);
                        TopWords.Remove(TopWords[z]);
                        z--;
                    }
                }
                PreviousListWordsCount = TopWords.Count();
                try { MaxInGroup =  WordsGroup.Max(x => x.Count()); } catch {}
            }
        }
        return Ok(Response.OrderBy(x => x.Position).ThenBy(x => x.Word).ToList());
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeletePDF([FromQuery] List<int> id)
    {
        List<PDF> ToBeDeleted = await Cache.GetCache("ListPDF", id);
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
                    await AzureServices.DeleteFile("sentences-container", delete.SentencesLinkTxt);
                }
                catch 
                {
                    ResponseMessage = ResponseMessage.AppendFormat("{0}) Id: {1} | Name: {2} | Blob file: Not found \n", x++, delete.id, delete.Name);
                }
            }
        }
        else{ return NotFound("No PDF(s) contain the submitted id(s)"); }
        await DB.SaveChangesAsync();
        return Ok(ResponseMessage.ToString());
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteAll()
    {
        List<PDF> ToBeDeleted = await Cache.GetCache("ListPDF", new List<int>());
        foreach(var delete in ToBeDeleted)
        {
            await AzureServices.DeleteFile("pdf-container", delete.FileLink);
            await AzureServices.DeleteFile("sentences-container", delete.SentencesLinkTxt);
        }
        DB.RemoveRange(ToBeDeleted);
        await DB.SaveChangesAsync();
        return Ok("All files were deleted");
    }
}