using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Link")]
    public class RichPreviewController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;

        private class MetadataGenerator
        {
            private Dictionary<string, string> _metaProperties = new Dictionary<string, string>();
            private string _redirect = String.Empty;
            private string _title = String.Empty;
            private string _description = String.Empty;

            public MetadataGenerator WithProperty(string property, string value)
            {
                _metaProperties.Add(property, value);
                return this;
            }
            public MetadataGenerator WithTitle(string value)
            {
                _title = value;
                _metaProperties.Add("og:title", value);
                return this;
            }

            public MetadataGenerator WithDescription(string value)
            {
                _description = value;
                _metaProperties.Add("og:description", value);
                return this;
            }
            public MetadataGenerator WithRedirect(string targetUrl)
            {
                _redirect = targetUrl;
                return this;
            }

            public string Render()
            {
                if (!_metaProperties.ContainsKey("og:type")) _metaProperties.Add("og:type", "website");

                var innerHtml = new StringBuilder();
                if (!String.IsNullOrWhiteSpace(_title)) innerHtml.Append($"<title>{_title}</title>");
                if (!String.IsNullOrWhiteSpace(_description)) innerHtml.Append($"<meta name=\"description\" content=\"{_description}\" />");
                if (!String.IsNullOrWhiteSpace(_redirect))
                    innerHtml.Append($"<meta http-equiv=\"refresh\" content=\"0;url={_redirect}\" />");

                foreach (var property in _metaProperties)
                    innerHtml.Append($"<meta property=\"{property.Key}\" content=\"{property.Value}\" />");

                return $"<html><head>{innerHtml}</head></html>";
            }

            public ActionResult AsResult()
            {
                return new ContentResult() { Content = Render(), ContentType = "text/html" };
            }
        }

        public RichPreviewController(
            IEventService eventService, 
            IEventConferenceDayService eventConferenceDayService)
        {
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
        }

        [HttpGet("Events/{Id}")]
        public async Task<ActionResult> GetEventById(Guid Id)
        {
            var @event = await _eventService.FindOneAsync(Id);
            if (@event == null) return NotFound();

            var eventConferenceDay = await _eventService.FindOneAsync(@event.ConferenceDayId);

            return new MetadataGenerator()
                .WithTitle(@event.Title)
                .WithDescription($"{eventConferenceDay.Title} {@event.StartTime}-{@event.EndTime}\n{@event.Description}")
                .AsResult();
        }

    }
}
