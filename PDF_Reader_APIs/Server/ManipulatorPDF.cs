using Spire.Pdf;
using System.Drawing;
using IronOcr;
using System.Text;
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

    public List<string> GetSentences(PdfDocument PdfFile)
    {
        string Pattern = "";
        List<string> Sentences = new List<string>();
        Match match;
        // StringBuilder Buffer = new StringBuilder();
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            match = Regex.Match(Page.ExtractText(), Pattern);
            if(match.Success)
            {
                Sentences.Add(match.Value);
            }
            else
            {
                Image[] PageImages = Page.ExtractImages();
                IronTesseract OCR = new IronTesseract();
                OcrInput Input = new OcrInput();
                foreach(var Image in PageImages)
                {
                    Input.AddImage(Image);
                    OcrResult Result = OCR.Read(Input);
                    Sentences.Add(Regex.Match(Result.Text, Pattern).Value);
                    Input = new OcrInput();
                }
            }
        }
        return Sentences;
    }  
}

