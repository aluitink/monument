namespace monument.api.client.Models
{
    public class PageSection
    {
        public string Body { get; set; }
    }
    public class Page
    {
        public string Name { get; set; }
        ICollection<PageSection> Sections { get; set; }

    }
    public class BlobGrant
    {
        public string BlobId { get; set; }
        public string UploadUri { get; set; }
    }
}
