using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class MapTests
{
    private MapRecord _record;

    private MapRequest _request;

    public MapTests()
    {
        var mapID = Guid.NewGuid();
        _record = new MapRecord()
        {
            Id = Guid.NewGuid(),
            Description = "Map description",
            Order = 1,
            IsBrowseable = true,
            ImageId = mapID,
            Image = new ImageRecord(),
            Entries = new List<MapEntryRecord>
            {
                new MapEntryRecord
                {
                    Id = Guid.NewGuid(),
                    X = 100,
                    Y = 200,
                    TapRadius = 50,
                    Links = new List<LinkFragment>
                    {
                        new LinkFragment { Target = "https://example.com", Name = "Example4444" }
                    },
                    MapId = mapID,
                    Map = new MapRecord()
                }
            }
        };

        _request = new MapRequest()
        {
            Description = "Map description",
            Order = 1,
            IsBrowseable = true,
            ImageId = Guid.NewGuid(),
            Entries = new List<MapEntryRequest>
            {
                new MapEntryRequest
                {
                    X = 100,
                    Y = 200,
                    TapRadius = 50,
                    Links = new List<LinkFragment>
                    {
                        new LinkFragment { Target = "https://example.com", Name = "Example2222" }
                    },
                }
            }
        };
    }

    [Fact]
    public void ValidateAdapterConfiguration()
    {
        var exception1 = Record.Exception(() =>
        {
            var config = TypeAdapterConfig<MapRequest, MapRecord>.NewConfig();
            config.Compile();
        });

        var exception2 = Record.Exception(() =>
        {
            var config2 = TypeAdapterConfig<MapRecord, MapResponse>.NewConfig();
            config2.Compile();
        });

        Assert.Null(exception1);
        Assert.Null(exception2);
    }


    [Fact]
    public void TestRequestToRecord()
    {
        var record = _request.Adapt<MapRecord>();
        AreEqual(record, _request);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        _request.Transform();

        var oldImage = _record.Image;

        _request.Description = "Something totally different";
        _request.Order = 1;
        _request.IsBrowseable = true;
        _request.ImageId = Guid.NewGuid();

        _record.Merge(_request);
        AreEqual(_record, _request);

        Assert.Equal(oldImage, _record.Image);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();
        AreEqual(res, _record);
    }

    private void AreEqual(MapRecord record, MapRequest req)
    {
        Assert.Equal(record.Description, req.Description);
        Assert.Equal(record.Order, req.Order);
        Assert.Equal(record.IsBrowseable, req.IsBrowseable);
        Assert.Equal(record.ImageId, req.ImageId);
    }

    private void AreEqual(MapResponse mapResponse, MapRecord record)
    {
        Assert.Equal(mapResponse.Id, record.Id);
        Assert.Equal(mapResponse.Description, record.Description);
        Assert.Equal(mapResponse.Order, record.Order);
        Assert.Equal(mapResponse.IsBrowseable, record.IsBrowseable);
        Assert.Equal(mapResponse.ImageId, record.ImageId);
        Assert.Equal(mapResponse.Entries.Count, record.Entries.Count);
        for (int i = 0; i < mapResponse.Entries.Count; i++)
        {
            Assert.Equal(mapResponse.Entries[i].Id, record.Entries[i].Id);
            Assert.Equal(mapResponse.Entries[i].X, record.Entries[i].X);
            Assert.Equal(mapResponse.Entries[i].Y, record.Entries[i].Y);
            Assert.Equal(mapResponse.Entries[i].TapRadius, record.Entries[i].TapRadius);
            Assert.Equal(mapResponse.Entries[i].Links, record.Entries[i].Links);
        }
    }
}
