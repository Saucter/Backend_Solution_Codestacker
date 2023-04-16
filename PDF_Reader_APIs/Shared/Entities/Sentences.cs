using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PDF_Reader_APIs.Shared.Entities
{
    /*
    This class was created as an EF Core workaround
    The PDF class was meant to have a list of strings -> List<string> Sentences
    However EF Core is unable to map some complex data types including lists of strings
    And so as a workaround this class was created and is connected to the PDF class through a One-To-Many relationship
    */
    public class Sentences
    {
        public Sentences(){}
        public Sentences(string Sentence)
        {
            this.Sentence = Sentence;
        }
        [Key]
        public int id {get; set;}
        public int PDFid {get; set;}
        public string Sentence {get; set;}
        [JsonIgnore]
        public PDF PDF {get; set;}
    }
}