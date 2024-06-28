namespace Eurofurence.App.Backoffice.Models
{
    public class KnowledgeEntry
    {
        public Guid Id { get; set; }
        public Guid KnowledgeGroupId { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public int Order { get; set; }
    }
}
