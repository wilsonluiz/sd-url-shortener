using System.ComponentModel.DataAnnotations.Schema;

namespace Labs.SystemDesign.UrlShortener
{
    [Table("url", Schema = "public")]
    public class Url
    {
        public string Id { get; set; }
        public string UrlLonga { get; set; }
        public string UrlCurta { get; set; }
        public DateTime? Criacao { get; set; }
        public bool? Ativo { get; set; }
    }
}
