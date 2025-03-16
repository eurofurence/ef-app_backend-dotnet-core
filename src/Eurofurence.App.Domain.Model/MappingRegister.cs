using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Eurofurence.App.Domain.Model.Announcements;
using Mapster;

namespace Eurofurence.App.Domain.Model

{

    public class MyRegister : ICodeGenerationRegister
    {
        public void Register(CodeGenerationConfig config)
        {
            config.AdaptTo("[name]Dto")
                .ForAllTypesInNamespace(Assembly.GetExecutingAssembly(), "Eurofurence.App.Domain.Model");

            config.GenerateMapper("[name]Mapper")
                .ForType<AnnounceMsg>();
        }
    }

    // public class MyRegister : IRegister
    // {
    //     // public void Register(TypeAdapterConfig config)
    //     // {
    //     //     config.NewConfig<TSource, TDestination>();
    //     // }
    // }
    // public class MappingRegister : ICodeGenerationRegister
    // {
    //     public void Register(CodeGenerationConfig config)
    //     {
    //         config.AdaptTo("[name]Dto", MapType.Map | MapType.MapToTarget | MapType.Projection)
    //             .ApplyDefaultRule()
    //             .AlterType<AnnouncementRecord, AnnouncementResponse>();
    //
    //         config.AdaptFrom("[name]Add", MapType.Map)
    //             .ApplyDefaultRule();
    //             //.IgnoreNoModifyProperties();
    //
    //             config.AdaptFrom("[name]Update", MapType.MapToTarget)
    //                 .ApplyDefaultRule()
    //                 .IgnoreAttributes(typeof(KeyAttribute));
    //             //.IgnoreNoModifyProperties();
    //
    //         config.AdaptFrom("[name]Merge", MapType.MapToTarget)
    //             .ApplyDefaultRule()
    //             .IgnoreAttributes(typeof(KeyAttribute))
    //            // .IgnoreNoModifyProperties()
    //             .IgnoreNullValues(true);
    //
    //         config.GenerateMapper("[name]Mapper")
    //             .ForType<AnnouncementRecord>()
    //             .ForType<AnnouncementRequest>()
    //             .ForType<AnnouncementResponse>();
    //     }
    //
    // }
    //
    // static class RegisterExtensions
    // {
    //     public static AdaptAttributeBuilder ApplyDefaultRule(this AdaptAttributeBuilder builder)
    //     {
    //         return builder
    //             .ForAllTypesInNamespace(Assembly.GetExecutingAssembly(), "Sample.CodeGen.Domains")
    //             //.ExcludeTypes(typeof(SchoolContext))
    //             .ExcludeTypes(type => type.IsEnum)
    //             .AlterType(type => type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true, typeof(string))
    //             .ShallowCopyForSameType(true);
    //         //.ForType<Enrollment>(cfg => cfg.Ignore(it => it.Course))
    //         //.ForType<Student>(cfg => cfg.Ignore(it => it.Enrollments));
    //     }
    //
    //     // public static AdaptAttributeBuilder IgnoreNoModifyProperties(this AdaptAttributeBuilder builder)
    //     // {
    //     //     return builder
    //     //         .ForType<Enrollment>(cfg => cfg.Ignore(it => it.Student));
    //     // }
    // }
}