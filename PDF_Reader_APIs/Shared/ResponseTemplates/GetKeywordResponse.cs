namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    //Class containing all the required responses in the HTTP GET response of the GetKeyword API
    public class GetKeyWordResponse
    {
        public GetKeyWordResponse(int PdfId, string PdfName, int Occurrences, List<string> Sentences, string Keyword, bool Exact, bool CaseSensitive)
        {
            this.PdfId = PdfId;
            this.PdfName = PdfName;
            this.Occurrences = Occurrences;
            this.Sentences = Sentences;
            this.Keyword = Keyword;
            this.Exact = Exact;
            this.CaseSensitive = CaseSensitive;
        }

        public int PdfId {get; set;}
        public string PdfName {get; set;}
        public string Keyword {get; set;}
        public int Occurrences   {get; set;}
        public int TotalOccurrences  {get; set;} //The total occurrances of the keyword across all the submitted IDs
        public bool Exact {get; set;}
        public bool CaseSensitive {get; set;}
        public List<string> Sentences {get; set;} 
    }
}
