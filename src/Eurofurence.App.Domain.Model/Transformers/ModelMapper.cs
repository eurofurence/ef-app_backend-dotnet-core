using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Riok.Mapperly.Abstractions;

namespace Eurofurence.App.Domain.Model.Transformers
{
    [Mapper]
    public static partial class ModelMapper
    {
        public static partial TTarget Map<TTarget>(object entityBase);

        public static partial void UpdateCarDto(AnnouncementRequest car, AnnouncementRecord dto);

        //public static partial void Merge<TSource, TTarger>(TSource source, TTarger target);

        // Mappings for Announcement
        private static partial AnnouncementRecord Map(AnnouncementRequest source);
        private static partial AnnouncementResponse Map(AnnouncementRecord source);

        // Mappings for PrivateMessage
        private static partial PrivateMessageRecord Map(SendPrivateMessageByIdentityRequest source);
        private static partial PrivateMessageRecord Map(SendPrivateMessageByRegSysRequest source);
        private static partial PrivateMessageResponse Map(PrivateMessageRecord source);

        // Mappings for Dealer
        private static partial DealerRecord Map(DealerRequest source);
        private static partial DealerResponse Map(DealerRecord source);


        private static partial EventRecord Map(EventRequest source);
        private static partial EventResponse Map(EventRecord source);


    }
}