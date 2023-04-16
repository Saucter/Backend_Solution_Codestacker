using System.Linq;
using Spire.Pdf;
using System.Drawing;
using Tesseract;
using System.Text;
using PDF_Reader_APIs.Shared.Entities;
using System.Text.RegularExpressions;
using StopWord;

//A static class containing additional methods that would needed in the controller class
public class ManipulatorPDF
{
    //Loads the PDF in a byte array format
    public static byte[] LoadBytePDF(IFormFile file)
    {
        MemoryStream ms = new MemoryStream();
        file.CopyTo(ms);
        return ms.ToArray();
    }  

    //Loads the PDF in a PdfDocument format (Given by Spire.Pdf)
    public static PdfDocument LoadPDF(IFormFile file)
    {
        byte[] FileInBytes = LoadBytePDF(file);
        PdfDocument PdfFile = new PdfDocument();
        PdfFile.LoadFromBytes(FileInBytes);
        return PdfFile;
    }

    //Removes any stop words (the, and, or, etc.) from a list of strings
    public static List<string> RemoveStopWords(List<string> ListSentences)
    {
        for(int i = 0; i < ListSentences.Count(); i++)
        {
            ListSentences[i] = ListSentences[i].RemoveStopWords("en"); //Utilizes the Stopwords library
        }
        return ListSentences;
    }

    //Splits a list of sentences into their individual words
    public static List<string> GetWords(List<Sentences> ListSentences) 
    {
        List<string> ListWords = new List<string>();
        foreach(var Sentence in ListSentences)
        {
            ListWords.AddRange(Sentence.Sentence.Split(new[] {" ", "-"}, StringSplitOptions.RemoveEmptyEntries)); //Splits the sentences at any whitespace or hyphen occurrence
        }
        for(int i = 0; i < ListWords.Count(); i++)
        {
            ListWords[i] = ListWords[i].ToLower(); //Makes all words lowercase
            if(char.IsPunctuation(ListWords[i][ListWords[i].Length-1]) && ListWords[i].Length != 1) //Checks if the word ends in a punctuation mark 
            {
                ListWords[i] = ListWords[i].Substring(0, ListWords[i].Length - 1); //Substitutes the word with a substring which excludes the punctuation
                i--; //Repeats the process one more in case the words has more than one punctuation mark at the end
            }
        }
        return ListWords;
    }

    //Places all sentences in a .txt file 
    //P.S: This can cause very long processing times
    public static byte[] SentencesToText(List<Sentences> ListSentences)
    {
        string FilePath = "./Sentences/Sentences.txt"; //Leads to an empty string
        MemoryStream ms = new MemoryStream();
        StreamWriter Writer = new StreamWriter(FilePath);
        foreach(var sentence in ListSentences)
        {
            Writer.WriteLine(sentence.Sentence); //Writes the sentence into the .txt file
        }
        Writer.Dispose(); //Diposes StreamWriter before giving the stream to FileStream
        FileStream fs = new FileStream(FilePath, FileMode.Open);
        fs.CopyTo(ms);  //Copies the stream to memory
        fs.SetLength(0); //Makes the .txt file empty
        fs.Dispose();
        return ms.ToArray(); //Returns the .txt file as a byte array
    }    

    //Parses the sentences in the PDF
    public static List<Sentences> GetSentences(PdfDocument PdfFile, bool? WithImages)
    {
        //Regex pattern for parsing sentences
        string StartOfSentence = @"\(?[A-Z][^.!?]*";
        string PreventDotNotation_NegativeLookahead = @"((\.|!|\?)(?! |\n|\r|\r\n)[^.!?]*)*";
        string EndOfSentence = @"(\.|!|\?)(?= |\n|\r|\r\n|)";
        string Pattern = new StringBuilder().AppendFormat("{0}{1}{2}", StartOfSentence, PreventDotNotation_NegativeLookahead, EndOfSentence).ToString();
        List<Sentences>? ListSentences = new List<Sentences>();
        List<string>? ListStrings = new List<string>();
        bool _WithImages = WithImages ?? false;

        //PdfDocument has PdfPageBase propetyu representing each page in the PDF doc
        foreach(PdfPageBase Page in PdfFile.Pages)
        {
            List<string> StringsInPage = Regex.Matches(Page.ExtractText(), Pattern).Cast<Match>().Select(m => m.Value.Trim()) //Matches the extracted text with the regex pattern
            .Where(x => !string.IsNullOrEmpty(x)) //Checks if the sentence is not null or empty
            .Concat((_WithImages) ? RegexOCR(Page, Pattern) : new List<string>()).ToList(); //Adds the text extracted from running OCR on images in the PDF (Important for image based PDFs)
            
            ListStrings.AddRange(FixBreaklines(StringsInPage)); //Fixes a bug related to breaklines in Spire.Pdf's text extraction and adds it to the final result's list
        }

        foreach(var sentence in ListStrings)
        {
            ListSentences.Add(new Sentences(sentence)); //Copnverts the strings into sentences 
        }
        return ListSentences;
    }

    //Implements OCR for images in the PDF 
    //P.S: This can be turned off if deeemd unnecessary, as it can significantly slow processing times
    public static List<string> RegexOCR(PdfPageBase Page, string Pattern)
    {
        List<string> StringSentences = new List<string>();
        Image[] PageImages = Page.ExtractImages(); //Extracts images from the PDF in the form of System.Drawing.Images
        var OcrEngine = new TesseractEngine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", EngineMode.Default); //Initiates OCR engine
        foreach(var PageImage in PageImages)
        {
            Page Image = OcrEngine.Process(PixConverter.ToPix((Bitmap) PageImage.Clone())); //Converts image from: Image -> Bitmap -> Pix | And runes the OCR on it afterwards
            string OcrText = Image.GetText();
            //Matches the extracted text with the RegEx pattern and returns an array of Match. It then trims the sentences and removes null strings
            StringSentences.AddRange(Regex.Matches(OcrText, Pattern).Cast<Match>().Select(m => m.Value.Trim()).Where(x => !string.IsNullOrEmpty(x))); 
            Image.Dispose(); //Disposes of the processed image
        }
        return StringSentences;
    }

    //Fixes a bug related to breaklines in Spire.Pdf's text extraction
    public static List<string> FixBreaklines(List<string> StringList)
    {
        StringBuilder Builder = new StringBuilder();
        List<string> SubStrings = new List<string>();

        foreach(var Sentence in StringList)
        {
            //Splits sentences at each occurrance of a breakline
            SubStrings.AddRange(Sentence.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        }

        for(int i = 0; i < SubStrings.Count() - 1; i++)
        {
            try
            {
                //Checks if the substring ends with a 'sentence-ending' punctuation mark and if the following sentence start with a capital letter
                if((!SubStrings[i].EndsWith(".") || !SubStrings[i].EndsWith("?") || !SubStrings[i].EndsWith("!")) && !char.IsUpper(SubStrings[i+1][0]))
                {
                    SubStrings[i] = Builder.AppendFormat("{0} {1}", SubStrings[i], SubStrings[i + 1]).ToString(); //Appends the two sentences together
                    SubStrings.RemoveAt(i+1); //Removes the second sentence from the list
                    i--; //Repeates the process one more time to check if the same condition applies to the newly concateenated sentence
                }
            }
            catch{}
        }
        return SubStrings;
    }
}