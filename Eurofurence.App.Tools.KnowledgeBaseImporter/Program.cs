using Autofac;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Eurofurence.App.Tools.KnowledgeBaseImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var _client = new MongoClient("mongodb://127.0.0.1:27017");
            var _database = _client.GetDatabase("app_dev");

            Domain.Model.MongoDb.BsonClassMapping.Register();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new Domain.Model.MongoDb.DependencyResolution.AutofacModule(_database));
            builder.RegisterModule(new Server.Services.DependencyResolution.AutofacModule());

            var container = builder.Build();

            //var password = File.ReadAllText(@"c:\temp\efapppw.txt");

            //var cookieContainer = new CookieContainer();

            //using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer }) 
            //using (var client = new HttpClient(handler))
            //{
            //    var pairs = new List<KeyValuePair<string, string>>
            //        {
            //            new KeyValuePair<string, string>("u", "ef_app"),
            //            new KeyValuePair<string, string>("p", password)
            //        };

            //    var loginUri = @"https://wiki.eurofurence.org/?do=login";
            //    var requestUri = @"https://wiki.eurofurence.org/doku.php?id=ef22:it:mobileapp:coninfo&do=export_raw";

            //    var loginPayload = new FormUrlEncodedContent(pairs);
            //    //var loginPayload = new StringContent($@"u=ef_app&p={password}");



            //    client.PostAsync(loginUri, loginPayload).Wait();
            //    var response = client.GetAsync(requestUri).Result;
            //    var content = response.Content.ReadAsStringAsync().Result;
            //}


            var importedKnowledgeGroups = new List<KnowledgeGroupRecord>();
            var importedKnowledgeEntries = new List<KnowledgeEntryRecord>();

            var content = File.ReadAllText(@"c:\temp\wikicont.txt");

            MatchCollection m;

            m = Regex.Matches(content, @"<WRAP.*?>PARSE_START</WRAP>(.*)?<WRAP.*?>PARSE_END</WRAP>", RegexOptions.Singleline);
            if (m.Count != 1) throw new InvalidDataException();

            m = Regex.Matches((m[0] as Match).Groups[1].Value, @"====([^=]+)====(.+?)((?=====)|$)", RegexOptions.Singleline);
            if (m.Count == 0) throw new InvalidDataException();

            int i = 0;
            foreach(Match matchGroup in m)
            {
                var titleParts = matchGroup.Groups[1].Value.Split('|').Select(a => a.Trim()).ToArray();
                var knowledgeGroup = new KnowledgeGroupRecord()
                {
                    Id = titleParts[0].AsHashToGuid(),
                    Name = titleParts[0],
                    Description = titleParts[1],
                    Order = i++
                };
                importedKnowledgeGroups.Add(knowledgeGroup);

                var entriesContent = matchGroup.Groups[2].Value;
                var entriesMatches = Regex.Matches(entriesContent, @"===([^=]+)===.+?<WRAP box[^>]*>(.+?)<\/WRAP>([^\<]*<WRAP lo[^>]*>([^\<]+)<\/WRAP>){0,1}", RegexOptions.Singleline);

                int j = 0;
                foreach(Match entryMatch in entriesMatches)
                {
                    var knowledgeEntry = new KnowledgeEntryRecord()
                    {
                        Id = entryMatch.Groups[1].Value.Trim().AsHashToGuid(),
                        KnowledgeGroupId = knowledgeGroup.Id,
                        Title = entryMatch.Groups[1].Value.Trim(),
                        Text = entryMatch.Groups[2].Value.Trim(),
                        Order = j++
                    };

                    importedKnowledgeEntries.Add(knowledgeEntry);
                }
            }

            UpdateKnowledgeGroups(importedKnowledgeGroups, container.Resolve<IKnowledgeGroupService>());
            UpdateKnowledgeEntries(importedKnowledgeEntries, container.Resolve<IKnowledgeEntryService>());
        }

        public static List<KnowledgeGroupRecord> UpdateKnowledgeGroups(
            IList<KnowledgeGroupRecord> importKnowledgeGroups,
            IKnowledgeGroupService service
            )
        {
            var knowledgeGroupRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<KnowledgeGroupRecord, KnowledgeGroupRecord>(
                (source, list) => list.SingleOrDefault(a => a.Id == source.Id)
                );

            patch
                .Map(s => s.Id, t => t.Id)
                .Map(s => s.Name, t => t.Name)
                .Map(s => s.Description, t => t.Description)
                .Map(s => s.Order, t => t.Order);

            var diff = patch.Patch(importKnowledgeGroups, knowledgeGroupRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public static List<KnowledgeEntryRecord> UpdateKnowledgeEntries(
            IList<KnowledgeEntryRecord> importKnowledgeEntries,
            IKnowledgeEntryService service
            )
        {
            var knowledgeEntryRecords = service.FindAllAsync().Result;

            var patch = new PatchDefinition<KnowledgeEntryRecord, KnowledgeEntryRecord>(
                (source, list) => list.SingleOrDefault(a => a.Id == source.Id)
                );

            patch
                .Map(s => s.Id, t => t.Id)
                .Map(s => s.KnowledgeGroupId, t => t.KnowledgeGroupId)
                .Map(s => s.Title, t => t.Title)
                .Map(s => s.Text, t => t.Text)
                .Map(s => s.Order, t => t.Order);

            var diff = patch.Patch(importKnowledgeEntries, knowledgeEntryRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }
    }
}