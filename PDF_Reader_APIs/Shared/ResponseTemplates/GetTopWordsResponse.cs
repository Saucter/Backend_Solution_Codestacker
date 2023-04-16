namespace PDF_Reader_APIs.Shared.ResponseTemplates
{
    //Class containing all the required responses in the HTTP GET response of the GetTopWords API
    public class GetTopWordsResponse
    {
        public GetTopWordsResponse(string Word, int Occurrences , int Position)
        {
            this.Word = Word;
            this.Occurrences  = Occurrences;
            this.Position = Position;
        }

        public string Word {get; set;}
        public int Occurrences {get; set;}
        public int Position {get; set;}
    }
}
