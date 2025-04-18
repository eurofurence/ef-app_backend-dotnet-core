using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class LostAndFoundTests
{
    private readonly LostAndFoundRecord _record;

    private readonly LostAndFoundRequest _request;

    public LostAndFoundTests()
    {
        _record = new LostAndFoundRecord()
        {
            Id = Guid.NewGuid(),
            ExternalId = 1,
            ImageUrl = "https://example.com/image.jpg",
            Title = "Lost Item",
            Description = "Description of the lost item.",
            Status = LostAndFoundRecord.LostAndFoundStatusEnum.Lost,
            LostDateTimeUtc = DateTime.UtcNow,
            FoundDateTimeUtc = null,
            ReturnDateTimeUtc = null
        };

        _request = new LostAndFoundRequest()
        {
            ExternalId = 1,
            ImageUrl = "https://example.com/image.jpg",
            Title = "Lost Item",
            Description = "Description of the lost item.",
            Status = LostAndFoundRecord.LostAndFoundStatusEnum.Lost,
            LostDateTimeUtc = DateTime.UtcNow,
            FoundDateTimeUtc = null,
            ReturnDateTimeUtc = null
        };
    }


    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var exception1 = Record.Exception((() =>
        {
            var config = TypeAdapterConfig<LostAndFoundRequest, LostAndFoundRecord>.NewConfig();
            config.Compile();
        }));

        var exception2 = Record.Exception((() =>
        {
            var config2 = TypeAdapterConfig<LostAndFoundRecord, LostAndFoundResponse>.NewConfig();
            config2.Compile();
        }));

        Assert.Null(exception1);
        Assert.Null(exception2);
    }


    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Adapt<LostAndFoundRecord>();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        var record = _request.Transform();
        AreEqual(record, _request);

        var oldGuid = _record.Id;

        _request.ImageUrl = "https://example.com/updated_image.jpg";
        _request.Title = "Updated Lost Item";
        _request.Description = "Updated description of the lost item.";
        _request.Status = LostAndFoundRecord.LostAndFoundStatusEnum.Found;
        _request.LostDateTimeUtc = DateTime.UtcNow.AddDays(-1);
        _request.FoundDateTimeUtc = DateTime.UtcNow;
        _request.ReturnDateTimeUtc = null;

        _record.Merge(_request);

        Assert.Equal(oldGuid, _record.Id);
        AreEqual(_record, _request);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.ImageUrl, res.ImageUrl);
        Assert.Equal(_record.Title, res.Title);
        Assert.Equal(_record.Description, res.Description);
        Assert.Equal(_record.Status, res.Status);
        Assert.Equal(_record.LostDateTimeUtc, res.LostDateTimeUtc);
        Assert.Equal(_record.FoundDateTimeUtc, res.FoundDateTimeUtc);
        Assert.Equal(_record.ReturnDateTimeUtc, res.ReturnDateTimeUtc);
        Assert.Equal(_record.ExternalId, res.ExternalId);
    }

    private static void AreEqual(LostAndFoundRecord record, LostAndFoundRequest request)
    {
        Assert.Equal(record.ExternalId, request.ExternalId);
        Assert.Equal(record.ImageUrl, request.ImageUrl);
        Assert.Equal(record.Title, request.Title);
        Assert.Equal(record.Description, request.Description);
        Assert.Equal(record.Status, request.Status);
        Assert.Equal(record.LostDateTimeUtc, request.LostDateTimeUtc);
        Assert.Equal(record.FoundDateTimeUtc, request.FoundDateTimeUtc);
        Assert.Equal(record.ReturnDateTimeUtc, request.ReturnDateTimeUtc);
    }
}
