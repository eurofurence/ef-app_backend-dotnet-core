using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class TableRegistrationRequestTests
{
    private TableRegistrationRequest _request;
    private TableRegistrationRecord _record;


    public TableRegistrationRequestTests()
    {
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
        var tableRegistrationResponse = _record.Transform();

        Assert.Equal(_record.DisplayName, tableRegistrationResponse.DisplayName);
        Assert.Equal(_record.ShortDescription, tableRegistrationResponse.ShortDescription);
        Assert.Equal(_record.Location, tableRegistrationResponse.Location);
        Assert.Equal(_record.WebsiteUrl, tableRegistrationResponse.WebsiteUrl);
        Assert.Equal(_record.TelegramHandle, tableRegistrationResponse.TelegramHandle);
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