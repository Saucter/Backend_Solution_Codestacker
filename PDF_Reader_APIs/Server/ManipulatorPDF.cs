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
        string Pattern = "^\\s+[A-Za-z,;'\"\\s]+[.?!]$";
        List<Sentences> ListSentences = new List<Sentences>();
        Match match;
        // StringBuilder Buffer = new StringBuilder();
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            match = Regex.Match(Page.ExtractText(), Pattern);
            if(match.Success)
            {
                ListSentences.Add(new Sentences(match.Value));
            }
            else
            {
                Image[] PageImages = Page.ExtractImages();
                var OcrEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                foreach(var PageImage in PageImages)
                {
                    Page Image = OcrEngine.Process(PixConverter.ToPix((Bitmap) PageImage.Clone()));
                    string OcrText = Image.GetText();
                    ListSentences.Add(new Sentences(Regex.Match(OcrText, Pattern).Value));
                }
            }
        }
        return ListSentences;
    }  
}

