using Spire.Pdf;
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
        StringBuilder Buffer = new StringBuilder();
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            Buffer.Append(Page.ExtractText());
        }
        Match match = Regex.Match(Buffer.ToString(), Pattern);


        return new List<string>();
    }  
}

