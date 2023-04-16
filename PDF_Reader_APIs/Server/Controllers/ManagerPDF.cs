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

//Set the class as an API controller with the route => localhost:\\pdf\{action}
[ApiController]
[Route("[controller]/[action]")]
[Authorize(AuthenticationSchemes = "BasicAuthentication")] //Enable basic authentication on this controller
public class pdfController : ControllerBase //Inherit from ControllerBase to use ActionResult
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


    //(POST) API to post a PDF to the database and parse its sentences
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<List<PDF>>> PostPDF(List<IFormFile> Files)
    {
        List<PDF> ListPDF = new List<PDF>();
        foreach(var file in Files)
        {
            if(System.IO.Path.GetExtension(file.FileName) == ".pdf") //Ensure the file being sent is a PDF
            {
                var FileInBytes = ManipulatorPDF.LoadBytePDF(file); //Byte array representing the PDF
                PdfDocument FileLoader = ManipulatorPDF.LoadPDF(file);
                List<Sentences> Sentences = ManipulatorPDF.GetSentences(FileLoader);
                
                //Create a new instance of a PDF with all the required parameters.
                //Save the file instance and sentences in an Azure blob storage using AzureSerices.SaveFile()
                PDF FileInstance = new PDF(file.FileName, file.Length, FileLoader.Pages.Count, Sentences, await AzureServices.SaveFile(FileInBytes, file.FileName, "pdf-container"),
                await AzureServices.SaveFile(ManipulatorPDF.SentencesToText(Sentences), file.FileName.Substring(0, file.FileName.Length - 4)+"_Sentence.txt", "sentences-container"));

                DB.Add(FileInstance); //Add an instance of the PDF class to the database
                ListPDF.Add(FileInstance); //Add instnace to a list
            }
            else
            {
                return BadRequest("Bad request: Only PDFs are accepted. File(s) sent is not a PDF"); //Return bad request message if a non-PDF is sent
            }
        }
        Cache.Remove("ListPDF"); //Unload the cache after a new post
        await DB.SaveChangesAsync();
        return ListPDF; //Return the list of PDFs representing a successful POST
    }


    //(GET) Get all PDFs or return a PDF based on id
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<GetPdfResponse>>> GetPDFs([FromQuery] List<int>? id, bool WithSentences)
    {
        List<PDF> ListPDFs = await Cache.GetCache("ListPDF", id); 
        List<GetPdfResponse> Response = new List<GetPdfResponse>();
        if(ListPDFs.Any(x => id.Contains(x.id)) || id.Count() == 0) //Checks if an unavilable ID is submitted
        {
            List<PDF> ToBeReturned = (id.Count() == 0) ? ListPDFs : ListPDFs.Where(x => id.Contains(x.id)).ToList(); //Query the requested IDs or all PDFs if id is null
            if(WithSentences) //Check if the request required sentences
            {
                return Ok(ToBeReturned);
            }
            else //If WithSentences is false, return a GetPdfResponse which does not return the sentences
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
            return NotFound("The PDF with submitted ID(s) is unavailable"); //Return response 404 if an unavailable ID is requested
        }       
    }


    //(GET) Gets all PDFs and sentences that contain a given keyword
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<GetKeyWordResponse>>> GetKeyword([FromQuery] List<int>? id, string Keyword, bool? Exact, bool? CaseSensitive)
    {
        List<GetKeyWordResponse> Response = new List<GetKeyWordResponse>();
        List<PDF> ListPDF = await Cache.GetCache("ListPDF", id);
        List<Sentences> ListSentences = new List<Sentences>();
        //Option variables are set to false by default
        bool _Exact = Exact ?? false;
        bool _CaseSensitive = CaseSensitive ?? false;

        if(!_CaseSensitive) //Turns everything to the same case if CaseSensitive is set to false or is null
        {
            Keyword = Keyword.ToLower();
            ListPDF.Select(pdf => //Selects the invididual PDFs with the sentences being in lowercase
            {
                pdf.Sentences.ForEach(s => s.Sentence = s.Sentence.ToLower());
                return pdf;
            });
        }   

        //If Exact is false then return any sentence which contains the sequence of letters in the keyword
        //If Exact is true then split the sentences to words and compare them to the keywords exactly
        ListSentences.AddRange((_Exact == false) ? ListPDF.SelectMany(s => s.Sentences.Where(x => x.Sentence.Contains(Keyword))).ToList(): 
        ListPDF.SelectMany(s => s.Sentences.Where(x => x.Sentence.Split(new[] {' ', '-', '\'', '\"', ','}, StringSplitOptions.RemoveEmptyEntries).Contains(Keyword))).ToList());
        //Select the PDFs which contain the sentence with the keyword by using the PDF id given in the one-to-many relationship mapper
        ListPDF = ListPDF.Where(x => ListSentences.Select(s => s.PDFid).Contains(x.id)).ToList(); 

        if(ListPDF.Count() != 0) //Check if any PDFs contain the Keyword
        {
            foreach(var pdf in ListPDF)
            {
                pdf.Sentences = ListSentences.Where(s => s.PDFid == pdf.id).ToList();
                Response.Add(new GetKeyWordResponse(pdf.id, pdf.Name, pdf.Sentences.Count(), pdf.Sentences.Select(x => x.Sentence).ToList(), Keyword, _Exact, _CaseSensitive));
            }
        }
        else //Return 404 response if not PDFs contain the keyword
        {
            return NotFound(new StringBuilder().AppendFormat("The keyword '{0}' is not avilable in the submitted PDF(s)", Keyword).ToString());
        }

        //Add the total occurrances in all PDFs as a response parameter to all response returns
        int Total = 0;
        Response.ForEach(x => Total += x.Occurrences);
        Response.ForEach(x => x.TotalOccurrences = Total);
        
        return Ok(Response);
    }


    //Return the most frequent words that occurred in the submitted PDF IDs
    public async Task<ActionResult<List<GetKeyWordResponse>>> GetTopWords([FromQuery] List<int>? id, int? NumberOfWords, [FromQuery] List<string>? Ignore)
    {
        List<GetTopWordsResponse> Response = new List<GetTopWordsResponse>();
        List<PDF> ListPDF = await Cache.GetCache("ListPDF", id);
        List<string> ListWords = ManipulatorPDF.GetWords(ListPDF.SelectMany(x => x.Sentences).ToList()); //Split sentences into lowercased words

        ListWords = ManipulatorPDF.RemoveStopWords(ListWords).Where(x => !string.IsNullOrEmpty(x)).ToList(); //Remove any stop words in the list of words (this, and, or, etc.)
        //Return a dictionary of the list of words grouped by name as their key
        //Make sure the key (word) is bigger than one and is an ASCII letter
        var WordsGroup = ListWords.GroupBy(x => x).Where(x => x.Key.Length > 1 && x.Key.ToCharArray().All(k => char.IsLetter(k))); 
        int MaxInGroup = WordsGroup.Max(x => x.Count()); //Return the count of the highest group
        
        List<string> TopWords = new List<string>();
        int? NumOfWords = (NumberOfWords == null) ? 5 : NumberOfWords; //Default 'NumberOfWords' to 5 if it is null
        int PreviousListWordsCount = 0;
        Ignore = Ignore.Select(x => x.ToLower()).ToList(); //Make sure the ignore list is in lowercased letters
        
        for(int i = 0; i < NumOfWords; i++) //Repeats based on the number of top words requested
        {
            var Word = WordsGroup.Where(x => x.Count() == MaxInGroup).Select(x => x.Key).ToList(); //Finds the group with the highest count and return its key
            if(Word != null) //Ensure that the MaxInGroup 
            {
                TopWords.AddRange(Word); //Add 
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