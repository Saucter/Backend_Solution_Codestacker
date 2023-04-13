namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    public class GetKeyWordResponse
    {
        public GetKeyWordResponse(string Keyword, bool Exact, bool CaseSensitive, int PdfId, string PdfName, List<string> Sentences, bool Success)
        {
            this.Keyword = Keyword;
            this.Exact = Exact;
            this.CaseSensitive = CaseSensitive;
            this.PdfId = PdfId;
            this.PdfName = PdfName;
            this.Sentences = Sentences;
            this.Success = Success;
        }

        public string Keyword {get; set;}
        public bool Exact {get; set;}
        public bool CaseSensitive {get; set;}
        public int PdfId {get; set;}
        public string PdfName {get; set;}
        public List<string> Sentences {get; set;} 
        public bool Success {get; set;}
    }
}
