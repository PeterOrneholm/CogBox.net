using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Orneholm.AIJukebox.Web
{
    public class ExcludeHealthDependencyFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExcludeHealthDependencyFilter(ITelemetryProcessor next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(ITelemetry item)
        {
            if (!OKtoSend(item))
            {
                return;
            }

            _next.Process(item);
        }

        private bool OKtoSend(ITelemetry item)
        {
            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request?.Path.Value.Equals("/health") ?? false)
            {
                return false;
            }

            return true;
        }
    }
}
