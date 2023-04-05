public class PDF
{
    public int id {get; set;}
    public string Name {get; set;}
    public DateTime TimeOfUpload {get; set;}
    public double FileSize {get; set;}
    public int NumberOfPages {get; set;}
    public string AzureLink {get; set;}
    public List<string> Sentences {get; set;}
}