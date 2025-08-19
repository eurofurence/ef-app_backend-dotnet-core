using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class ImageTests
{
    private ImageRequest _request;

    private ImageRecord _record;

    public ImageTests()
    {
        TypeAdapterConfig typeAdapterConfig;
        typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Default.PreserveReference(true);
        typeAdapterConfig.Scan(typeof(ImageRecord).Assembly);

        _request = new ImageRequest()
        {
            InternalReference = "InternalReference",
            Width = 100,
            Height = 100,
            SizeInBytes = 100,
            MimeType = "MimeType",
            ContentHashSha1 = "ContentHashSha1",
            Url = "Url",
            IsRestricted = true,
        };

        _record = new ImageRecord()
        {
            Id = Guid.NewGuid(),
            InternalReference = "InternalReference",
            Width = 100,
            Height = 100,
            SizeInBytes = 100,
            MimeType = "MimeType",
            ContentHashSha1 = "ContentHashSha1",
            Url = "Url",
            IsRestricted = true,
            InternalFileName = "internalFileName.gif",
            Announcements = new List<AnnouncementRecord>()
            {
                new AnnouncementRecord()
                {
                    Id = Guid.NewGuid(),
                    Area = "Main Hall",
                    Author = "Dr. Gordon Freeman",
                    Title = "Zen cristal experiment",
                    Content = "Please report all of your results to Dr. Breen",
                    ImageId = Guid.NewGuid(),
                    ValidFromDateTimeUtc = DateTime.UtcNow,
                    ValidUntilDateTimeUtc = DateTime.UtcNow.AddDays(1),
                    ExternalReference = "asdf",
                    Image = new ImageRecord(),
                }
            },
            KnowledgeEntries = new List<KnowledgeEntryRecord>()
            {
                new KnowledgeEntryRecord()
                {
                    Id = Guid.NewGuid(),
                    KnowledgeGroupId = Guid.NewGuid(),
                    Title = "Knowledge Entry Title",
                    Text = "Knowledge Entry Text",
                    Order = 1,
                    Links = new List<LinkFragment>(),
                }
            },
            TableRegistrations = new List<TableRegistrationRecord>()
            {
                new TableRegistrationRecord()
                {
                    Id = Guid.NewGuid(),
                    ImageId = Guid.NewGuid(),
                    Image = new ImageRecord(),
                }
            },
            Maps = new List<MapRecord>()
            {
                new MapRecord()
                {
                    Id = Guid.NewGuid(),
                }
            },
            EventBanners = new List<EventRecord>()
            {
                new EventRecord()
                {
                    Id = Guid.NewGuid(),
                    Title = "Event Banner",
                    Description = "Event Banner Description",
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                }
            },
            EventPosters = new List<EventRecord>()
            {
                new EventRecord()
                {
                    Id = Guid.NewGuid(),
                    Title = "Event Poster",
                    Description = "Event Poster Description",
                    StartDateTimeUtc = DateTime.UtcNow,
                    EndDateTimeUtc = DateTime.UtcNow.AddDays(1),
                }
            },
            DealerArtists = new List<DealerRecord>()
            {
                new DealerRecord()
                {
                    Id = Guid.NewGuid(),
                    ArtistImageId = Guid.NewGuid(),
                    ArtistImage = new ImageRecord(),
                }
            },
            DealerArtistThumbnails = new List<DealerRecord>()
            {
                new DealerRecord()
                {
                    Id = Guid.NewGuid(),
                    ArtistThumbnailImageId = Guid.NewGuid(),
                    ArtistThumbnailImage = new ImageRecord(),
                }
            },
            DealerArtPreviews = new List<DealerRecord>()
            {
                new DealerRecord()
                {
                    Id = Guid.NewGuid(),
                    ArtPreviewImageId = Guid.NewGuid(),
                    ArtPreviewImage = new ImageRecord(),
                }
            },
            BlurHash = "BlurHashValue",
        };
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var exception1 = Record.Exception(() =>
        {
            var config = TypeAdapterConfig<ImageRequest, ImageRecord>.NewConfig();
            config.Compile();
        });

        var exception2 = Record.Exception(() =>
        {
            var config2 = TypeAdapterConfig<ImageRecord, ImageWithRelationsResponse>.NewConfig();
            config2.Compile();
        });

        Assert.Null(exception1);
        Assert.Null(exception2);
    }

    [Fact]
    public void TestRequestToRecord()
    {
        ImageRecord record = _request.Transform();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        var oldGuid = _record.Id;

        var oldAnnouncements = _record.Announcements;
        var oldKnowledgeEntries = _record.KnowledgeEntries;
        var oldTableRegistrations = _record.TableRegistrations;
        var oldMaps = _record.Maps;
        var oldEventBanners = _record.EventBanners;
        var oldEventPosters = _record.EventPosters;
        var oldDealerArtists = _record.DealerArtists;
        var oldDealerArtistThumbnails = _record.DealerArtistThumbnails;
        var oldDealerArtPreviews = _record.DealerArtPreviews;
        var oldInternalFileName = _record.InternalFileName;
        var oldBlurHash = _record.BlurHash;

        _request.InternalReference = "Something totally different";
        _request.Width = 200;
        _request.Height = 200;
        _request.SizeInBytes = 200;
        _request.MimeType = "Something totally different";
        _request.ContentHashSha1 = "Something totally different";
        _request.Url = "Something totally different";
        _request.IsRestricted = false;

        _record.Merge<ImageRequest, ImageWithRelationsResponse, ImageRecord>(_request);

        Assert.Equal(oldGuid, _record.Id);
        Assert.Equal(oldAnnouncements, _record.Announcements);
        Assert.Equal(oldKnowledgeEntries, _record.KnowledgeEntries);
        Assert.Equal(oldTableRegistrations, _record.TableRegistrations);
        Assert.Equal(oldMaps, _record.Maps);
        Assert.Equal(oldEventBanners, _record.EventBanners);
        Assert.Equal(oldEventPosters, _record.EventPosters);
        Assert.Equal(oldDealerArtists, _record.DealerArtists);
        Assert.Equal(oldDealerArtistThumbnails, _record.DealerArtistThumbnails);
        Assert.Equal(oldDealerArtPreviews, _record.DealerArtPreviews);
        Assert.Equal(oldInternalFileName, _record.InternalFileName);
        Assert.Equal(oldBlurHash, _record.BlurHash);

        AreEqual(_record, _request);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform<ImageWithRelationsResponse>();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.InternalReference, res.InternalReference);
        Assert.Equal(_record.Width, res.Width);
        Assert.Equal(_record.Height, res.Height);
        Assert.Equal(_record.SizeInBytes, res.SizeInBytes);
        Assert.Equal(_record.MimeType, res.MimeType);
        Assert.Equal(_record.ContentHashSha1, res.ContentHashSha1);
        Assert.Equal(_record.Url, res.Url);
        Assert.Equal(_record.IsRestricted, res.IsRestricted);
        Assert.Equal(_record.BlurHash, res.BlurHash);

        Assert.Equal(_record.Announcements.Select(x => x.Id), res.AnnouncementIds);
        Assert.Equal(_record.KnowledgeEntries.Select(x => x.Id), res.KnowledgeEntryIds);
        Assert.Equal(_record.TableRegistrations.Select(x => x.Id), res.TableRegistrationIds);
        Assert.Equal(_record.Maps.Select(x => x.Id), res.MapIds);
        Assert.Equal(_record.EventBanners.Select(x => x.Id), res.EventBannerIds);
        Assert.Equal(_record.EventPosters.Select(x => x.Id), res.EventPosterIds);
        Assert.Equal(_record.DealerArtists.Select(x => x.Id), res.DealerArtistIds);
        Assert.Equal(_record.DealerArtistThumbnails.Select(x => x.Id), res.DealerArtistThumbnailIds);
        Assert.Equal(_record.DealerArtPreviews.Select(x => x.Id), res.DealerArtPreviewIds);
    }

    private void AreEqual(ImageRecord record, ImageRequest request)
    {
        Assert.Equal(record.InternalReference, request.InternalReference);
        Assert.Equal(record.Width, request.Width);
        Assert.Equal(record.Height, request.Height);
        Assert.Equal(record.SizeInBytes, request.SizeInBytes);
        Assert.Equal(record.MimeType, request.MimeType);
        Assert.Equal(record.ContentHashSha1, request.ContentHashSha1);
        Assert.Equal(record.Url, request.Url);
        Assert.Equal(record.IsRestricted, request.IsRestricted);
    }
}