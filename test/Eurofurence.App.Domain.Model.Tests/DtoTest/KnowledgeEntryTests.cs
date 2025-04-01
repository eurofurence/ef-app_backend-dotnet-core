using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class KnowledgeEntryTests
{
    private KnowledgeEntryRecord _record;
    private KnowledgeEntryRequest _request;

    public KnowledgeEntryTests()
    {
        _request = new KnowledgeEntryRequest()
        {
            KnowledgeGroupId = System.Guid.NewGuid(),
            Title = "Knowledge Entry Title",
            Text = "Knowledge Entry Text",
            Order = 1,
            Links = new(),
            ImageIds = new(),

        };

        _record = new KnowledgeEntryRecord()
        {
            Id = System.Guid.NewGuid(),
            KnowledgeGroupId = System.Guid.NewGuid(),
            Title = "Knowledge Entry Title",
            Text = "Knowledge Entry Text",
            Order = 1,
            Links = new(),

        };
    }
}
