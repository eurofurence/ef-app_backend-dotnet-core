using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

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
    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var exception1 = Record.Exception((() =>
        {
            var config = TypeAdapterConfig<KnowledgeEntryRequest, KnowledgeEntryRecord>.NewConfig();
            config.Compile();
        }));

        var exception2 = Record.Exception((() =>
        {
            var config2 = TypeAdapterConfig<KnowledgeEntryRecord, KnowledgeEntryResponse>.NewConfig();
            config2.Compile();
        }));

        Assert.Null(exception1);
        Assert.Null(exception2);
    }

    [Fact]
    public void TestRequestToRecord()
    {
        KnowledgeEntryRecord record = _request.Transform();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        var oldGuid = _record.Id;

        _request.KnowledgeGroupId = System.Guid.NewGuid();
        _request.Title = "Something totally different";
        _request.Text = "Something totally different";
        _request.Order = 1;
        _request.Links.Add(new LinkFragment()
        {
            Id = System.Guid.NewGuid(),
            Name = "Something totally different",
            Target = "Something totally different",
        });


        _request.ImageIds.Add(System.Guid.NewGuid());

        _record.Merge(_request);
        Assert.Equal(oldGuid, _record.Id);
        AreEqual(_record, _request);
    }

    private void AreEqual(KnowledgeEntryRecord record, KnowledgeEntryRequest request)
    {
        Assert.Equal(record.KnowledgeGroupId, request.KnowledgeGroupId);
        Assert.Equal(record.Title, request.Title);
        Assert.Equal(record.Text, request.Text);
        Assert.Equal(record.Order, request.Order);
        Assert.Equal(record.Links.Count, request.Links.Count);
        Assert.Equal(record.Images.Select(x => x.Id), request.ImageIds);
    }
}
