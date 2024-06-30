using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;

namespace Eurofurence.App.Tools.CliToolBox.Importers.Knowledge
{
    public class WikiFileImporter
    {
        private readonly IKnowledgeGroupService _knowledgeGroupService;
        private readonly IKnowledgeEntryService _knowledgeEntryService;
        private readonly IImageService _imageService;

        public WikiFileImporter(IKnowledgeGroupService knowledgeGroupService, IKnowledgeEntryService knowledgeEntryService, IImageService imageService)
        {
            _knowledgeGroupService = knowledgeGroupService;
            _knowledgeEntryService = knowledgeEntryService;
            _imageService = imageService;
        }

        public async Task<int> ImportWikiFileAsync(string inputPath)
        {
            var importedKnowledgeGroups = new List<KnowledgeGroupRecord>();
            var importedKnowledgeEntries = new List<KnowledgeEntryRecord>();

            var content = File.ReadAllText(inputPath);

            MatchCollection m;

            m = Regex.Matches(content, @"<WRAP.*?>PARSE_START</WRAP>(.*)?<WRAP.*?>PARSE_END</WRAP>",
                RegexOptions.Singleline);
            if (m.Count != 1) throw new InvalidDataException();

            m = Regex.Matches(m[0].Groups[1].Value, @"====([^=]+)====(.+?)((?=====)|$)", RegexOptions.Singleline);
            if (m.Count == 0) throw new InvalidDataException();

            var i = 0;
            foreach (Match matchGroup in m)
            {
                var titleParts = matchGroup.Groups[1].Value.Split('|').Select(a => a.Trim()).ToList();
                while (titleParts.Count < 3) titleParts.Add("");

                var knowledgeGroup = new KnowledgeGroupRecord
                {
                    Id = titleParts[0].AsHashToGuid(),
                    Name = titleParts[0],
                    Description = titleParts[1],
                    FontAwesomeIconName = titleParts[2],
                    Order = i++
                };
                importedKnowledgeGroups.Add(knowledgeGroup);

                var entriesContent = matchGroup.Groups[2].Value;
                var entriesMatches = Regex.Matches(entriesContent,
                    @"===([^=]+)===.+?<WRAP box[^>]*>(.+?)<\/WRAP>([^\<]*<WRAP lo[^>]*>([^\<]+)<\/WRAP>){0,1}",
                    RegexOptions.Singleline);

                var j = 0;
                foreach (Match entryMatch in entriesMatches)
                {
                    var knowledgeEntry = new KnowledgeEntryRecord
                    {
                        Id = $"{entryMatch.Groups[1].Value.Trim()}-{knowledgeGroup.Id}".AsHashToGuid(),
                        KnowledgeGroupId = knowledgeGroup.Id,
                        Title = entryMatch.Groups[1].Value.Trim(),
                        Text = entryMatch.Groups[2].Value.Trim(),
                        Order = j++
                    };

                    await ProcessLinksAsync(knowledgeEntry, entryMatch.Groups[3].Value.Trim());

                    importedKnowledgeEntries.Add(knowledgeEntry);
                }
            }

            int modifiedRecords = 0;

            var knowledgeGroups = UpdateKnowledgeGroups(importedKnowledgeGroups, _knowledgeGroupService, ref modifiedRecords);
            var knowledgeEntries = UpdateKnowledgeEntries(importedKnowledgeEntries, _knowledgeEntryService, ref modifiedRecords);
            modifiedRecords += (await CleanUpImagesAsync(knowledgeEntries));

            return modifiedRecords;
        }

        private async Task<int> CleanUpImagesAsync(List<KnowledgeEntryRecord> knowledgeEntries)
        {
            int modifiedRecords = 0;
            var usedImageIds = knowledgeEntries.SelectMany(a => a.Images).Select(image => image.Id).Distinct().ToList();

#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
            var knowledgeImages =
                _imageService.FindAll(a => a.IsDeleted == 0 &&
                                                      a.InternalReference.StartsWith("knowledge:"));
#pragma warning restore RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.

            foreach (var image in knowledgeImages)
            {
                if (!usedImageIds.Contains(image.Id))
                {
                    await _imageService.DeleteOneAsync(image.Id);
                    modifiedRecords += 1;
                }
            }

            return modifiedRecords;
        }

        private async Task ProcessLinksAsync(KnowledgeEntryRecord knowledgeEntry, string url)
        {
            MatchCollection m = Regex.Matches(url, @"  \* \[\[([^\|]+)\|([^\]]+)\]\]", RegexOptions.Singleline);

            var images = new List<ImageRecord>();
            var linkFragments = new List<LinkFragment>();

            foreach (Match linkMatch in m)
            {
                var target = linkMatch.Groups[1].Value.Trim();
                var name = linkMatch.Groups[2].Value.Trim();


                if (name.Equals("image", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Get the image...
                    try
                    {
                        var imageBytes = await GetImageAsync(target);
                        using var ms = new MemoryStream(imageBytes);
                        var image =
                            await _imageService.InsertImageAsync($"knowledge:{url.AsHashToGuid()}", ms);

                        images.Add(image);
                    }
                    catch (HttpRequestException)
                    {
                        
                    }
                }
                else
                {
                    if (!Uri.IsWellFormedUriString(target, UriKind.Absolute) &&
                        Uri.IsWellFormedUriString($"http://{target}", UriKind.Absolute))
                        target = $"http://{target}";

                    if (Uri.IsWellFormedUriString(target, UriKind.Absolute))
                    {
                        linkFragments.Add(new LinkFragment()
                        {
                            FragmentType = LinkFragment.FragmentTypeEnum.WebExternal,
                            Name = name,
                            Target = target
                        });
                    }
                }
            }

            knowledgeEntry.Links = linkFragments;
            knowledgeEntry.Images = images;
        }

        private async Task<byte[]> GetImageAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetByteArrayAsync(url);
                return result;
            }
        }

        public List<KnowledgeGroupRecord> UpdateKnowledgeGroups(
            IList<KnowledgeGroupRecord> importKnowledgeGroups,
            IKnowledgeGroupService service,
            ref int modifiedRecords
        )
        {
            var knowledgeGroupRecords = service.FindAll();

            var patch = new PatchDefinition<KnowledgeGroupRecord, KnowledgeGroupRecord>(
                (source, list) => list.SingleOrDefault(a => a.Id == source.Id)
            );

            patch
                .Map(s => s.Id, t => t.Id)
                .Map(s => s.Name, t => t.Name)
                .Map(s => s.Description, t => t.Description)
                .Map(s => s.FontAwesomeIconName, t => t.FontAwesomeIconName)
                .Map(s => s.Order, t => t.Order);

            var diff = patch.Patch(importKnowledgeGroups, knowledgeGroupRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }

        public List<KnowledgeEntryRecord> UpdateKnowledgeEntries(
            IList<KnowledgeEntryRecord> importKnowledgeEntries,
            IKnowledgeEntryService service,
            ref int modifiedRecords
        )
        {
            var knowledgeEntryRecords = service.FindAll();

            var patch = new PatchDefinition<KnowledgeEntryRecord, KnowledgeEntryRecord>(
                (source, list) => list.SingleOrDefault(a => a.Id == source.Id)
            );

            patch
                .Map(s => s.Id, t => t.Id)
                .Map(s => s.KnowledgeGroupId, t => t.KnowledgeGroupId)
                .Map(s => s.Title, t => t.Title)
                .Map(s => s.Text, t => t.Text)
                .Map(s => s.Order, t => t.Order)
                .Map(s => s.Links, t => t.Links)
                .Map(s => s.Images, t => t.Images);

            var diff = patch.Patch(importKnowledgeEntries, knowledgeEntryRecords);

            service.ApplyPatchOperationAsync(diff).Wait();

            modifiedRecords += diff.Count(a => a.Action != ActionEnum.NotModified);
            return diff.Where(a => a.Entity.IsDeleted == 0).Select(a => a.Entity).ToList();
        }
    }
}