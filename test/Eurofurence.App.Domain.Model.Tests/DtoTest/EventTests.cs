using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class EventTests
{
    private EventRequest _request;
    private EventRecord _record;

    public EventTests()
    {
        _request = new EventRequest()
        {
            Title = "Event Title",
            Abstract = "Event Abstract",
            ConferenceDayId = System.Guid.NewGuid(),
            ConferenceRoomId = System.Guid.NewGuid(),
            ConferenceTrackId = System.Guid.NewGuid(),
            Description = "Event Description",
            Duration = System.TimeSpan.FromMinutes(60),
            EndDateTimeUtc = System.DateTime.UtcNow,
            EndTime = System.TimeSpan.FromHours(12),
            IsAcceptingFeedback = true,
            IsDeviatingFromConBook = true,
            PanelHosts = "Panel Hosts",
            Slug = "Event Slug",
            StartDateTimeUtc = System.DateTime.UtcNow,
            StartTime = System.TimeSpan.FromHours(10),
            SubTitle = "Event SubTitle",
            Tags = ["Tag1", "Tag2"],
            BannerImageId = Guid.NewGuid(),
            PosterImageId = Guid.NewGuid(),
        };
        _record = new EventRecord()
        {
            Id = Guid.NewGuid(),
            Title = "Event Title",
            Abstract = "Event Abstract",
            ConferenceDayId = System.Guid.NewGuid(),
            ConferenceRoomId = System.Guid.NewGuid(),
            ConferenceTrackId = System.Guid.NewGuid(),
            Description = "Event Description",
            Duration = System.TimeSpan.FromMinutes(60),
            EndDateTimeUtc = System.DateTime.UtcNow,
            EndTime = System.TimeSpan.FromHours(12),
            IsAcceptingFeedback = true,
            IsDeviatingFromConBook = true,
            PanelHosts = "Panel Hosts",
            Slug = "Event Slug",
            StartDateTimeUtc = System.DateTime.UtcNow,
            StartTime = System.TimeSpan.FromHours(10),
            SubTitle = "Event SubTitle",
            Tags = ["Tag1", "Tag2"],
            BannerImageId = Guid.NewGuid(),
            PosterImageId = Guid.NewGuid(),
        };
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var config = TypeAdapterConfig<EventRequest, EventRecord>.NewConfig();
        config.Compile();
        var config2 = TypeAdapterConfig<EventRecord, EventResponse>.NewConfig();
        config2.Compile();
    }


    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Adapt<EventRecord>();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        EventRecord record = _request.Transform();

        var oldGuid = _record.Id;

        _request.Title = "Something totally different";
        _request.Abstract = "Something totally different";
        _request.ConferenceDayId = System.Guid.NewGuid();
        _request.ConferenceRoomId = System.Guid.NewGuid();
        _request.ConferenceTrackId = System.Guid.NewGuid();
        _request.Description = "Something totally different";
        _request.Duration = System.TimeSpan.FromMinutes(120);
        _request.EndDateTimeUtc = System.DateTime.UtcNow;
        _request.EndTime = System.TimeSpan.FromHours(14);
        _request.IsAcceptingFeedback = false;
        _request.IsDeviatingFromConBook = false;
        _request.PanelHosts = "Something totally different";
        _request.Slug = "Something totally different";
        _request.StartDateTimeUtc = System.DateTime.UtcNow;
        _request.StartTime = System.TimeSpan.FromHours(12);
        _request.SubTitle = "Something totally different";
        _request.Tags = ["Tag3", "Tag4"];
        _request.BannerImageId = Guid.NewGuid();
        _request.PosterImageId = Guid.NewGuid();

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
        Assert.Equal(_record.Abstract, res.Abstract);
        Assert.Equal(_record.ConferenceDayId, res.ConferenceDayId);
        Assert.Equal(_record.ConferenceRoomId, res.ConferenceRoomId);
        Assert.Equal(_record.ConferenceTrackId, res.ConferenceTrackId);
        Assert.Equal(_record.Description, res.Description);
        Assert.Equal(_record.Duration, res.Duration);
        Assert.Equal(_record.EndDateTimeUtc, res.EndDateTimeUtc);
        Assert.Equal(_record.EndTime, res.EndTime);
        Assert.Equal(_record.IsAcceptingFeedback, res.IsAcceptingFeedback);
        Assert.Equal(_record.IsDeviatingFromConBook, res.IsDeviatingFromConBook);
        Assert.Equal(_record.PanelHosts, res.PanelHosts);
        Assert.Equal(_record.Slug, res.Slug);
        Assert.Equal(_record.StartDateTimeUtc, res.StartDateTimeUtc);
        Assert.Equal(_record.StartTime, res.StartTime);
        Assert.Equal(_record.SubTitle, res.SubTitle);
        Assert.Equal(_record.Tags, res.Tags);
        Assert.Equal(_record.BannerImageId, res.BannerImageId);
        Assert.Equal(_record.PosterImageId, res.PosterImageId);
    }

    private void AreEqual(EventRecord record, EventRequest request)
    {
        Assert.Equal(record.Title, request.Title);
        Assert.Equal(record.Abstract, request.Abstract);
        Assert.Equal(record.ConferenceDayId, request.ConferenceDayId);
        Assert.Equal(record.ConferenceRoomId, request.ConferenceRoomId);
        Assert.Equal(record.ConferenceTrackId, request.ConferenceTrackId);
        Assert.Equal(record.Description, request.Description);
        Assert.Equal(record.Duration, request.Duration);
        Assert.Equal(record.EndDateTimeUtc, request.EndDateTimeUtc);
        Assert.Equal(record.EndTime, request.EndTime);
        Assert.Equal(record.IsAcceptingFeedback, request.IsAcceptingFeedback);
        Assert.Equal(record.IsDeviatingFromConBook, request.IsDeviatingFromConBook);
        Assert.Equal(record.PanelHosts, request.PanelHosts);
        Assert.Equal(record.Slug, request.Slug);
        Assert.Equal(record.StartDateTimeUtc, request.StartDateTimeUtc);
        Assert.Equal(record.StartTime, request.StartTime);
        Assert.Equal(record.SubTitle, request.SubTitle);
        Assert.Equal(record.Tags, request.Tags);
        Assert.Equal(record.BannerImageId, request.BannerImageId);
        Assert.Equal(record.PosterImageId, request.PosterImageId);
    }
}