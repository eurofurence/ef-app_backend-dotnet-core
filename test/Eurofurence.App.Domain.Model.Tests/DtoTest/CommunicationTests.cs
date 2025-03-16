using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Xunit;

namespace Eurofurence.App.Domain.Model.Tests.DtoTest;

public class CommunicationTests
{
    private PrivateMessageRecord _record;
    private SendPrivateMessageByIdentityRequest _ipdRequest;
    private SendPrivateMessageByRegSysRequest _regSysRequest;

    public CommunicationTests()
    {
        _record = new PrivateMessageRecord()
        {
            Id = Guid.NewGuid(),
            Message = "John Wick. excommunicado in 1 hour",
            AuthorName = "Winston",
            Subject = "Worldwide",
            SenderUid = "23243535",
            RecipientIdentityId = "89349395",
            RecipientRegSysId = "42",
            CreatedDateTimeUtc = DateTime.Now,
            ReceivedDateTimeUtc = DateTime.Now.AddSeconds(2),
            ReadDateTimeUtc = DateTime.Now.AddHours(1),
        };

        _ipdRequest = new SendPrivateMessageByIdentityRequest()
        {
            Message = "John Wick. excommunicado in 1 hour",
            AuthorName = "Winston",
            Subject = "Worldwide",
            RecipientUid = "23243535",
            ToastTitle = "Important message",
            ToastMessage = "New excommunicado was declared",
        };

        _regSysRequest = new SendPrivateMessageByRegSysRequest()
        {
            Message = "John Wick. excommunicado in 1 hour",
            AuthorName = "Winston",
            Subject = "Worldwide",
            RecipientUid = "23243535",
            ToastTitle = "Important message",
            ToastMessage = "New excommunicado was declared",
        };
    }

    [Fact]
    public void TestRequestToRecord()
    {
        var record1 = _ipdRequest.Transform();
        var record2 = _regSysRequest.Transform();

        AreEqual(record1, _ipdRequest);
        AreEqual(record2, _regSysRequest);
    }

    [Fact]
    public void TestRequestMergeIntoRecord()
    {
        var recordIdp = _ipdRequest.Transform();
        PrivateMessageRecord recordReg = _regSysRequest.Transform();

        var oldGuidIpd = recordIdp.Id;
        var oldGuidReg = recordReg.Id;

        _ipdRequest.Message = "Something totally different";
        _ipdRequest.AuthorName = "Something totally different";
        _ipdRequest.Subject = "Something totally different";
        _ipdRequest.RecipientUid = "Something totally different";
        _ipdRequest.ToastTitle = "Something totally different";
        _ipdRequest.ToastMessage = "Something totally different";

        recordIdp.Merge(_ipdRequest);

        _regSysRequest.Message = "Something totally different";
        _regSysRequest.AuthorName = "Something totally different";
        _regSysRequest.Subject = "Something totally different";
        _regSysRequest.RecipientUid = "Something totally different";
        _regSysRequest.ToastTitle = "Something totally different";
        _regSysRequest.ToastMessage = "Something totally different";

        recordReg.Merge(_regSysRequest);

        Assert.Equal(oldGuidIpd, recordIdp.Id);
        Assert.Equal(oldGuidReg, recordReg.Id);
        AreEqual(recordReg, _regSysRequest);
        AreEqual(recordIdp, _ipdRequest);
    }

    [Fact]
    public void TestRecordIntoResponse()
    {
        var res = _record.Transform();

        Assert.Equal(_record.Id, res.Id);
        Assert.Equal(_record.Message, res.Message);
        Assert.Equal(_record.AuthorName, res.AuthorName);
        Assert.Equal(_record.Subject, res.Subject);
        Assert.Equal(_record.SenderUid, res.SenderUid);
        Assert.Equal(_record.RecipientIdentityId, res.RecipientIdentityId);
        Assert.Equal(_record.RecipientRegSysId, res.RecipientRegSysId);
        Assert.Equal(_record.CreatedDateTimeUtc, res.CreatedDateTimeUtc);
        Assert.Equal(_record.ReceivedDateTimeUtc, res.ReceivedDateTimeUtc);
        Assert.Equal(_record.ReadDateTimeUtc, res.ReadDateTimeUtc);
    }

    private void AreEqual(PrivateMessageRecord record, SendPrivateMessageByIdentityRequest ipdRequest)
    {
        Assert.Equal(record.Message, ipdRequest.Message);
        Assert.Equal(record.AuthorName, ipdRequest.AuthorName);
        Assert.Equal(record.Subject, ipdRequest.Subject);
    }

    private void AreEqual(PrivateMessageRecord record, SendPrivateMessageByRegSysRequest regRequest)
    {
        Assert.Equal(record.Message, regRequest.Message);
        Assert.Equal(record.AuthorName, regRequest.AuthorName);
        Assert.Equal(record.Subject, regRequest.Subject);
    }
}