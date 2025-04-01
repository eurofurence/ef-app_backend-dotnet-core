using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class ImageTests
{
    private ImageRequest _request;

    private ImageRecord _record;

    public ImageTests()
    {
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
        };
    }

    [Fact]
    public void ValidateTypeAdapterConfiguration()
    {
        var config = TypeAdapterConfig<ImageRequest, ImageRecord>.NewConfig();
        config.Compile();
        var config2 = TypeAdapterConfig<ImageRecord, ImageResponse>.NewConfig();
        config2.Compile();
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

        _request.InternalReference = "Something totally different";
        _request.Width = 200;
        _request.Height = 200;
        _request.SizeInBytes = 200;
        _request.MimeType = "Something totally different";
        _request.ContentHashSha1 = "Something totally different";
        _request.Url = "Something totally different";
        _request.IsRestricted = false;

        _record.Merge(_request);

        Assert.Equal(oldGuid, _record.Id);
        AreEqual(_record, _request);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.InternalReference, res.InternalReference);
        Assert.Equal(_record.Width, res.Width);
        Assert.Equal(_record.Height, res.Height);
        Assert.Equal(_record.SizeInBytes, res.SizeInBytes);
        Assert.Equal(_record.MimeType, res.MimeType);
        Assert.Equal(_record.ContentHashSha1, res.ContentHashSha1);
        Assert.Equal(_record.Url, res.Url);
        Assert.Equal(_record.IsRestricted, res.IsRestricted);
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
