using System.Linq;
using Spire.Pdf;
using System.Drawing;
using Tesseract;
using System.Text;
using PDF_Reader_APIs.Shared.Entities;
using System.Text.RegularExpressions;

public class ManipulatorPDF
{
    public static byte[] LoadBytePDF(IFormFile file)
    {
        MemoryStream ms = new MemoryStream();
        file.CopyTo(ms);
        return ms.ToArray();
    }  

    public static PdfDocument LoadPDF(IFormFile file)
    {
        byte[] FileInBytes = LoadBytePDF(file);
        PdfDocument PdfFile = new PdfDocument();
        PdfFile.LoadFromBytes(FileInBytes);
        return PdfFile;
    }  

    public static List<Sentences> GetSentences(PdfDocument PdfFile)
    {
        string Pattern = @"\(?[A-Z][^.!?]*((\.|!|\?)(?! |\n|\r|\r\n)[^.!?]*)*(\.|!|\?)(?= |\n|\r|\r\n|)";
        List<Sentences>? ListSentences = new List<Sentences>();
        List<string>? ListStrings = new List<string>();
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            List<string> StringsInPage = Regex.Matches(Page.ExtractText(), Pattern).Cast<Match>().Select(m => m.Value.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .Concat(RegexOCR(Page, Pattern)).ToList();
            
            ListStrings.AddRange(FixBreaklines(StringsInPage));
        }
        foreach(var sentence in ListStrings)
        {
            ListSentences.Add(new Sentences(sentence));
        }
        return ListSentences;
    }

    public static List<string> RegexOCR(PdfPageBase Page, string Pattern)
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

    public static List<string> FixBreaklines(List<string> StringList)
    {
        List<string> SubStrings = new List<string>();

        foreach(var Sentence in StringList)
        {
            SubStrings.AddRange(Sentence.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
        }
        for(int i = 0; i < SubStrings.Count() - 1; i++)
        {
            try
            {
                if((!SubStrings[i].EndsWith(".") || !SubStrings[i].EndsWith("?") || !SubStrings[i].EndsWith("!")) && !char.IsUpper(SubStrings[i+1][0]))
                {
                    SubStrings[i] = string.Concat(SubStrings[i], SubStrings[i+1]);
                    SubStrings.RemoveAt(i+1);
                    i--;
                }
            }
            catch{}
        }
        return SubStrings;
    }
}