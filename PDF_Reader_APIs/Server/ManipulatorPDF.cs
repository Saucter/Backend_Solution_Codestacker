using System.Linq;
using Spire.Pdf;
using System.Drawing;
using Tesseract;
using System.Text;
using PDF_Reader_APIs.Shared.Entities;
using System.Text.RegularExpressions;

public class ManipulatorPDF
{
    public PdfDocument LoadPDF(IFormFile file)
    {
        byte[] FileInBytes;
        MemoryStream ms = new MemoryStream();
        file.CopyTo(ms);
        FileInBytes = ms.ToArray();
        PdfDocument PdfFile = new PdfDocument();
        PdfFile.LoadFromBytes(FileInBytes);
        return PdfFile;
    }  

    public List<Sentences> GetSentences(PdfDocument PdfFile)
    {
        string Pattern = @"\(?[A-Z][^.!?]*((\.|!|\?)(?! |\n|\r|\r\n)[^.!?]*)*(\.|!|\?)(?= |\n|\r|\r\n|)";
        List<Sentences>? ListSentences = new List<Sentences>();
        List<string>? ListStrings = new List<string>();
        MatchCollection Matches;
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            
            ListStrings.AddRange(Regex.Matches(Page.ExtractText(), Pattern).Cast<Match>().Select(m => m.Value.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Concat(RegexOCR(Page, Pattern)));
        }
        foreach(var sentence in ListStrings)
        {
            ListSentences.Add(new Sentences(sentence));
        }
        return ListSentences;
    }

    public List<string> RegexOCR(PdfPageBase Page, string Pattern)
    {
        List<string> StringSentences = new List<string>();
        Image[] PageImages = Page.ExtractImages();
        var OcrEngine = new TesseractEngine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", EngineMode.Default);
        foreach(var PageImage in PageImages)
        {
            Page Image = OcrEngine.Process(PixConverter.ToPix((Bitmap) PageImage.Clone()));
            string OcrText = Image.GetText();
            StringSentences.AddRange(Regex.Matches(OcrText, Pattern).Cast<Match>().Select(m => m.Value.Trim()).Where(x => !string.IsNullOrEmpty(x)));
            Image.Dispose();
        }
        return StringSentences;
    }
}