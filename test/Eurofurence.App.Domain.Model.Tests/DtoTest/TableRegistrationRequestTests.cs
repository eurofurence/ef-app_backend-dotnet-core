using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class TableRegistrationRequestTests
{
    private readonly TableRegistrationRequest _request;
    private readonly TableRegistrationRecord _record;

    public TableRegistrationRequestTests()
    {
        TypeAdapterConfig typeAdapterConfig;
        typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Default.PreserveReference(true);
        typeAdapterConfig.Scan(typeof(TableRegistrationRecord).Assembly);

        _request = new TableRegistrationRequest()
        {
            DisplayName = "John Doe",
            ShortDescription = "Sketches and illustrations",
            TelegramHandle = "@jdoe",
            WebsiteUrl = "https://www.example.com",
            Location = "Table 42"
        };

        _record = new TableRegistrationRecord()
        {
            DisplayName = "John Doe",
            ShortDescription = "Sketches and illustrations",
            TelegramHandle = "@jdoe",
            WebsiteUrl = "https://www.example.com",
            Location = "Table 42",
            ImageId = Guid.NewGuid(),
            Id = Guid.NewGuid(),
            State = TableRegistrationRecord.RegistrationStateEnum.Pending,
            Image = new ImageRecord()
            {
                Id = Guid.NewGuid(),
                InternalReference = "image-internal-reference",
                Width = 100,
                Height = 100,
                SizeInBytes = 1024,
                MimeType = "image/png",
                ContentHashSha1 = "sha1hash",
                Url = "https://example.com/image.png",
                IsRestricted = false,
            },
            OwnerUid = "bla bla",
            OwnerUsername = "doejohn",
        };
    }

    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Transform();
        AreEqual(record, _request);
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var config = TypeAdapterConfig<TableRegistrationRequest, TableRegistrationRecord>.NewConfig();
        config.Compile();
        var config2 = TypeAdapterConfig<TableRegistrationRecord, TableRegistrationResponse>.NewConfig();
        config2.Compile();
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        var record = _request.Transform();
        AreEqual(record, _request);

        var oldGuid = _record.Id;

        _request.DisplayName = "something different";
        _request.ShortDescription = "something different";
        _request.TelegramHandle = "@jdoe";
        _request.Location = "Table 42";
        _record.WebsiteUrl = "something different";

        _record.Merge(_request);

        Assert.Equal(oldGuid, _record.Id);
        AreEqual(_record, _request);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var tableRegistrationResponse = _record.Transform<TableRegistrationResponse>();

        Assert.Equal(_record.DisplayName, tableRegistrationResponse.DisplayName);
        Assert.Equal(_record.ShortDescription, tableRegistrationResponse.ShortDescription);
        Assert.Equal(_record.Location, tableRegistrationResponse.Location);
        Assert.Equal(_record.WebsiteUrl, tableRegistrationResponse.WebsiteUrl);
        Assert.Equal(_record.TelegramHandle, tableRegistrationResponse.TelegramHandle);
        Assert.Equal(_record.ImageId, tableRegistrationResponse.ImageId);
        Assert.Equal(_record.State, tableRegistrationResponse.State);
        Assert.Equal(_record.Id, tableRegistrationResponse.Id);
        Assert.Equal(_record.OwnerUid, tableRegistrationResponse.OwnerUid);
        Assert.Equal(_record.OwnerUsername, tableRegistrationResponse.OwnerUsername);
        Assert.Equal(tableRegistrationResponse.Image, _record.Image.Transform<ImageResponse>());
        Assert.Equal(_record.ImageId, tableRegistrationResponse.ImageId);
    }

    private void AreEqual(TableRegistrationRecord record, TableRegistrationRequest req)
    {
        Assert.Equal(record.DisplayName, req.DisplayName);
        Assert.Equal(record.ShortDescription, req.ShortDescription);
        Assert.Equal(record.Location, req.Location);
        Assert.Equal(record.WebsiteUrl, req.WebsiteUrl);
        Assert.Equal(record.TelegramHandle, req.TelegramHandle);
    }
}