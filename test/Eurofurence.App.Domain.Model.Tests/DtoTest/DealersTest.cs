using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class DealersTest
{
    private DealerRecord _record;
    private DealerRequest _request;

    public DealersTest()
    {
        _record = new DealerRecord
        {
            RegistrationNumber = 12345,
            AttendeeNickname = "artist123",
            DisplayName = "Amazing Artworks",
            Merchandise = "Prints, Stickers, and Original Art",
            ShortDescription = "A variety of art and merchandise from Amazing Artworks.",
            AboutTheArtistText = "Amazing Artworks is a collective of talented artists.",
            AboutTheArtText = "We offer a wide range of art styles and merchandise.",
            Links = new List<LinkFragment>
            {
                new LinkFragment {  Target = "https://amazingartworks.com", Name = "Official Website" }
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

        _request = new DealerRequest
        {
            DisplayName = "Amazing Artworks",
            Merchandise = "Prints, Stickers, and Original Art",
            ShortDescription = "A variety of art and merchandise from Amazing Artworks.",
            AboutTheArtistText = "Amazing Artworks is a collective of talented artists.",
            AboutTheArtText = "We offer a wide range of art styles and merchandise.",
            Links = new List<LinkFragment>
            {
                new LinkFragment {  Target = "https://amazingartworks.com", Name = "Official Website" }
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
        var config = TypeAdapterConfig<DealerRequest, DealerRecord>.NewConfig();
        config.Compile();
        var config2 = TypeAdapterConfig<DealerRecord, DealerResponse>.NewConfig();
        config2.Compile();
    }

    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Adapt<DealerRecord>();
        AreEqual(record, _request);
    }


    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        DealerRecord record = _request.Transform(); ;

        var oldGuid = record.Id;

        _request.DisplayName = "Something totally different";
        _request.Merchandise = "Something totally different";
        _request.ShortDescription = "Something totally different";
        _request.AboutTheArtistText = "Something totally different";
        _request.AboutTheArtText = "Something totally different";
        _request.TwitterHandle = "Something totally different";
        _request.TelegramHandle = "Something totally different";
        _request.DiscordHandle = "Something totally different";
        _request.MastodonHandle = "Something totally different";

        record.Merge(_request);

        Assert.Equal(oldGuid, record.Id);
        AreEqual(record, _request);
    }

    private void AreEqual(DealerRecord record, DealerRequest request)
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