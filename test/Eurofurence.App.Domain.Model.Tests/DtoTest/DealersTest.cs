using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class DealersTest
{
    private readonly DealerRecord _record;
    private readonly DealerRequest _request;

    public DealersTest()
    {
        _record = new DealerRecord
        {
            Id = Guid.NewGuid(),
            DisplayName = "Amazing Artworks",
            Merchandise = "Prints, Stickers, and Original Art",
            ShortDescription = "A variety of art and merchandise from Amazing Artworks.",
            AboutTheArtistText = "Amazing Artworks is a collective of talented artists.",
            AboutTheArtText = "We offer a wide range of art styles and merchandise.",
            Links = new List<LinkFragment>
            {
                new LinkFragment { Target = "https://amazingartworks.com", Name = "Official Website" }
            },
            TwitterHandle = "@amazingartworks",
            TelegramHandle = "@amazingartworks",
            DiscordHandle = "amazingartworks#1234",
            MastodonHandle = "@amazingartworks@mastodon.social",
            BlueskyHandle = "@amazingartworks.bsky.social",
            AttendsOnThursday = true,
            AttendsOnFriday = true,
            AttendsOnSaturday = false,
            ArtPreviewCaption = "Check out our latest art preview!",
            ArtistThumbnailImageId = Guid.NewGuid(),
            ArtistImageId = Guid.NewGuid(),
            ArtPreviewImageId = Guid.NewGuid(),
            IsAfterDark = false,
            Categories = new[] { "Art", "Merchandise" },
            Keywords = new Dictionary<string, string[]>
            {
                { "Art", new[] { "Digital", "Traditional" } },
                { "Merchandise", new[] { "Prints", "Stickers" } }
            },
            ArtistThumbnailImage = new ImageRecord(),
            ArtPreviewImage = new ImageRecord(),
        };

        _request = new DealerRequest
        {
            DisplayName = "Amazing Artworks",
            Merchandise = "Prints, Stickers, and Original Art",
            ShortDescription = "A variety of art and merchandise from Amazing Artworks.",
            AboutTheArtistText = "Amazing Artworks is a collective of talented artists.",
            AboutTheArtText = "We offer a wide range of art styles and merchandise.",
            Links = new List<LinkFragment>
            {
                new LinkFragment { Target = "https://amazingartworks.com", Name = "Official Website" }
            },
            TwitterHandle = "@amazingartworks",
            TelegramHandle = "@amazingartworks",
            DiscordHandle = "amazingartworks#1234",
            MastodonHandle = "@amazingartworks@mastodon.social",
            BlueskyHandle = "@amazingartworks.bsky.social",
            AttendsOnThursday = true,
            AttendsOnFriday = true,
            AttendsOnSaturday = false,
            ArtPreviewCaption = "Check out our latest art preview!",
            ArtistThumbnailImageId = Guid.NewGuid(),
            ArtistImageId = Guid.NewGuid(),
            ArtPreviewImageId = Guid.NewGuid(),
            IsAfterDark = false,
            Categories = new[] { "Art", "Merchandise" },
            Keywords = new Dictionary<string, string[]>
            {
                { "Art", new[] { "Digital", "Traditional" } },
                { "Merchandise", new[] { "Prints", "Stickers" } }
            }
        };
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var exception1 = Record.Exception(() =>
        {
            var config = TypeAdapterConfig<DealerRequest, DealerRecord>.NewConfig();
            config.Compile();
        });

        var exception2 = Record.Exception(() =>
        {
            var config2 = TypeAdapterConfig<DealerRecord, DealerResponse>.NewConfig();
            config2.Compile();
        });

        Assert.Null(exception1);
        Assert.Null(exception2);
    }

    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Adapt<DealerRecord>();
        AreEqual(record, _request);
        Assert.Equal(record.DisplayName, _request.DisplayName);
    }


    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        _request.Transform();

        var oldGuid = _record.Id;
        var oldArtistImage = _record.ArtistImage;
        var oldArtistThumbnailImage = _record.ArtistThumbnailImage;

        var oldArtPreviewImage = _record.ArtPreviewImage;

        _request.DisplayName = "Something totally different";
        _request.Merchandise = "Something totally different";
        _request.ShortDescription = "Something totally different";
        _request.AboutTheArtistText = "Something totally different";
        _request.AboutTheArtText = "Something totally different";
        _request.TwitterHandle = "Something totally different";
        _request.TelegramHandle = "Something totally different";
        _request.DiscordHandle = "Something totally different";
        _request.MastodonHandle = "Something totally different";
        _request.ArtistImageId = Guid.NewGuid();
        _request.ArtistThumbnailImageId = Guid.NewGuid();

        _record.Merge(_request);

        Assert.Equal(oldGuid, _record.Id);
        Assert.Equal(oldArtistImage, _record.ArtistImage);
        Assert.Equal(oldArtistThumbnailImage, _record.ArtistThumbnailImage);
        Assert.Equal(oldArtPreviewImage, _record.ArtPreviewImage);

        AreEqual(_record, _request);
    }


    [Fact]
    public void TestRequestMergeIntoRecordUnaffectedByRecordChanges()
    {
        var oldArtistThumbnailImage = _record.ArtistThumbnailImage.Id;
        var oldArtPreviewImage = _record.ArtPreviewImage.Id;

        _record.ArtistImage = new ImageRecord();
        _record.ArtPreviewImage = new ImageRecord();

        _record.Merge(_request);

        Assert.NotEqual(oldArtistThumbnailImage, _record.ArtistImage.Id);
        Assert.NotEqual(oldArtPreviewImage, _record.ArtPreviewImage.Id);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.DisplayName, res.DisplayName);
        Assert.Equal(_record.Merchandise, res.Merchandise);
        Assert.Equal(_record.ShortDescription, res.ShortDescription);
        Assert.Equal(_record.AboutTheArtistText, res.AboutTheArtistText);
        Assert.Equal(_record.AboutTheArtText, res.AboutTheArtText);
        Assert.Equal(_record.Links, res.Links);
        Assert.Equal(_record.TwitterHandle, res.TwitterHandle);
        Assert.Equal(_record.TelegramHandle, res.TelegramHandle);
        Assert.Equal(_record.DiscordHandle, res.DiscordHandle);
        Assert.Equal(_record.MastodonHandle, res.MastodonHandle);
        Assert.Equal(_record.BlueskyHandle, res.BlueskyHandle);
        Assert.Equal(_record.AttendsOnThursday, res.AttendsOnThursday);
        Assert.Equal(_record.AttendsOnFriday, res.AttendsOnFriday);
        Assert.Equal(_record.AttendsOnSaturday, res.AttendsOnSaturday);
        Assert.Equal(_record.ArtPreviewCaption, res.ArtPreviewCaption);
        Assert.Equal(_record.ArtistThumbnailImageId, res.ArtistThumbnailImageId);
        Assert.Equal(_record.ArtistImageId, res.ArtistImageId);
        Assert.Equal(_record.ArtPreviewImageId, res.ArtPreviewImageId);
        Assert.Equal(_record.IsAfterDark, res.IsAfterDark);
        Assert.Equal(_record.Categories, res.Categories);
        Assert.Equal(_record.Keywords, res.Keywords);
    }


    private static void AreEqual(DealerRecord record, DealerRequest request)
    {
        Assert.Equal(record.DisplayName, request.DisplayName);
        Assert.Equal(record.Merchandise, request.Merchandise);
        Assert.Equal(record.ShortDescription, request.ShortDescription);
        Assert.Equal(record.AboutTheArtistText, request.AboutTheArtistText);
        Assert.Equal(record.AboutTheArtText, request.AboutTheArtText);
        Assert.Equal(record.Links, request.Links);
        Assert.Equal(record.TwitterHandle, request.TwitterHandle);
        Assert.Equal(record.TelegramHandle, request.TelegramHandle);
        Assert.Equal(record.DiscordHandle, request.DiscordHandle);
        Assert.Equal(record.MastodonHandle, request.MastodonHandle);
        Assert.Equal(record.BlueskyHandle, request.BlueskyHandle);
        Assert.Equal(record.AttendsOnThursday, request.AttendsOnThursday);
        Assert.Equal(record.AttendsOnFriday, request.AttendsOnFriday);
        Assert.Equal(record.AttendsOnSaturday, request.AttendsOnSaturday);
        Assert.Equal(record.ArtPreviewCaption, request.ArtPreviewCaption);
        Assert.Equal(record.ArtistThumbnailImageId, request.ArtistThumbnailImageId);
        Assert.Equal(record.ArtistImageId, request.ArtistImageId);
        Assert.Equal(record.ArtPreviewImageId, request.ArtPreviewImageId);
        Assert.Equal(record.IsAfterDark, request.IsAfterDark);
        Assert.Equal(record.Categories, request.Categories);
        Assert.Equal(record.Keywords, request.Keywords);
    }
}
