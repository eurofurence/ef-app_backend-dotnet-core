using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class AnnouncementsTests
{
    private AnnouncementRecord _record;
    private AnnouncementRequest _request;

    public AnnouncementsTests()
    {
        _request = new AnnouncementRequest()
        {
            Area = "Main Hall",
            Author = "Dr. Gordon Freeman",
            Title = "Zen cristal experiment",
            Content = "Please report all of your results to Dr. Breen",
            ImageId = Guid.NewGuid(),
        };

        _record = new AnnouncementRecord()
        {
            Area = "Main Hall",
            Author = "Dr. Gordon Freeman",
            Title = "Zen cristal experiment",
            Content = "Please report all of your results to Dr. Breen",
            ImageId = Guid.NewGuid(),
            Id = Guid.NewGuid(),
            ValidFromDateTimeUtc = new DateTime(),
            ValidUntilDateTimeUtc = new DateTime(),
            ExternalReference = "asdf",
            Image = new ImageRecord(),

        };
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var exception1 = Record.Exception((() =>
        {
            var config = TypeAdapterConfig<AnnouncementRequest, AnnouncementRecord>.NewConfig();
            config.Compile();
        }));

        var exception2 = Record.Exception((() =>
         {
             var config2 = TypeAdapterConfig<AnnouncementRecord, AnnouncementResponse>.NewConfig();
             config2.Compile();
         }));

        Assert.Null(exception1);
        Assert.Null(exception2);
    }

    [Fact]
    public void TestRequestToRecord()
    {
        AnnouncementRecord record = _request.Transform();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        AnnouncementRecord record = _request.Transform();
        AreEqual(record, _request);

        var oldGuid = _record.Id;

        _request.Title = "Something totally different";
        _request.Area = "Something totally different";
        _request.Author = "Something totally different";
        _request.Content = "Something totally different";
        _request.ImageId = Guid.NewGuid();


        _record.Merge(_request);
        Assert.Equal(oldGuid, _record.Id);
        AreEqual(_record, _request);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.Title, res.Title);
        Assert.Equal(_record.Author, res.Author);
        Assert.Equal(_record.Content, res.Content);
        Assert.Equal(_record.Area, res.Area);
        Assert.Equal(_record.ImageId, res.ImageId);
    }

    private void AreEqual(AnnouncementRecord record, AnnouncementRequest req)
    {
        Assert.Equal(record.Title, req.Title);
        Assert.Equal(record.Author, req.Author);
        Assert.Equal(record.Content, req.Content);
        Assert.Equal(record.Area, req.Area);
        Assert.Equal(record.ImageId, req.ImageId);
    }
}
