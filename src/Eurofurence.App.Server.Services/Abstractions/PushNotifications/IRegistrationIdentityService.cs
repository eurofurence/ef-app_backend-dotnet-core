using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications;

public interface IRegistrationIdentityService :
    IEntityServiceOperations<UserRecord>,
    IPatchOperationProcessor<UserRecord>;