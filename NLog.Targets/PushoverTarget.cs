using NLog.Config;
using NLog.Layouts;
using NPushover;
using NPushover.RequestObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NLog.Targets
{
    [Target("Pushover")]
    public class PushoverTarget : TargetWithLayout
    {
        private static readonly IList<PushoverPriorityRule> defaultPriorityRules = new List<PushoverPriorityRule>()
        {
            new PushoverPriorityRule("level == LogLevel.Fatal", Priority.Emergency, Sounds.Siren, 30, 86400),
            new PushoverPriorityRule("level == LogLevel.Error", Priority.High),
            new PushoverPriorityRule("level == LogLevel.Warn", Priority.High),
            new PushoverPriorityRule("level == LogLevel.Info", Priority.Normal),
            new PushoverPriorityRule("level == LogLevel.Debug", Priority.Low),
            new PushoverPriorityRule("level == LogLevel.Trace", Priority.Lowest),
        };

        private Pushover _pushover;
        private Encoding _encoding;
        private Uri _baseuri;

        public PushoverTarget()
        {
            this.Subject = "Message from NLog on ${machinename}";
            this.PriorityRules = new List<PushoverPriorityRule>();
            this.UseDefaultPriorityRules = true;
            this.Encoding = Pushover.DEFAULTENCODING.BodyName;
            this.BaseUri = Pushover.DEFAULTBASEURI.AbsoluteUri;
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            _baseuri = new Uri(this.BaseUri);
            _encoding = System.Text.Encoding.GetEncoding(this.Encoding);
            _pushover = new Pushover(this.ApplicationKey, _baseuri, _encoding);
        }

        [DefaultValue(true)]
        public bool UseDefaultPriorityRules { get; set; }

        [ArrayParameter(typeof(PushoverPriorityRule), "priority-rule")]
        public IList<PushoverPriorityRule> PriorityRules { get; private set; }

        [RequiredParameter]
        public string ApplicationKey { get; set; }

        public string BaseUri { get; set; }

        public string Encoding { get; set; }

        [DefaultValue(false)]
        public bool Html { get; set; }

        [DefaultValue("Message from NLog on ${machinename}")]
        public Layout Subject { get; set; }

        [RequiredParameter]
        public Layout UserOrGroupName { get; set; }

        public Layout DeviceName { get; set; }

        public string SuplementaryURL { get; set; }
        public string SuplementaryURLTitle { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            //Find a matching priority rule
            var matchingRule = MatchPriorityRule(logEvent, this.PriorityRules);
            if (matchingRule == null && this.UseDefaultPriorityRules)
                matchingRule = MatchPriorityRule(logEvent, defaultPriorityRules);
            //None found? Use default
            if (matchingRule == null)
                matchingRule = PushoverPriorityRule.Default;

            //Build message
            var message = new Message(matchingRule.Sound)
            {
                Body = this.Layout.Render(logEvent).Trim(),
                IsHtmlBody = this.Html,
                Priority = matchingRule.Priority,
                Timestamp = logEvent.TimeStamp,
                Title = GetWithFallback(matchingRule.Subject, this.Subject).Render(logEvent).Trim(),
            };

            if (matchingRule.RetryIntervalSeconds > 0 && matchingRule.RetryPeriodSeconds > 0)
            {
                message.RetryOptions = new RetryOptions
                {
                    RetryEvery = TimeSpan.FromSeconds(matchingRule.RetryIntervalSeconds),
                    RetryPeriod = TimeSpan.FromSeconds(matchingRule.RetryPeriodSeconds),
                    CallBackUrl = string.IsNullOrEmpty(matchingRule.RetryCallbackURL) ? null : new Uri(matchingRule.RetryCallbackURL)
                };
            }

            var supurl = GetWithFallback(matchingRule.SuplementaryURL, this.SuplementaryURL);
            var supurltitle = GetWithFallback(matchingRule.SuplementaryURLTitle, this.SuplementaryURLTitle);

            if (!string.IsNullOrEmpty(supurl))
            {
                message.SupplementaryUrl = new SupplementaryURL
                {
                    Uri = new Uri(supurl),
                    Title = supurltitle
                };
            }

            var userorgrouplayout = GetWithFallback(matchingRule.UserOrGroupName, this.UserOrGroupName);
            var devicelayout = GetWithFallback(matchingRule.DeviceName, this.DeviceName);

            var result = _pushover.SendMessageAsync(
                message,
                userorgrouplayout.Render(logEvent).Trim(),
                devicelayout == null ? new string[0] : new[] { devicelayout.Render(logEvent).Trim() }
            ).Result;
        }

        private static PushoverPriorityRule MatchPriorityRule(LogEventInfo logEvent, IList<PushoverPriorityRule> rules)
        {
            foreach (var ppr in rules)
                if (ppr.CheckCondition(logEvent))
                    return ppr;
            return null;
        }

        private static T GetWithFallback<T>(T value, T fallbackValue, T defaultValue = null)
            where T: class
        {
            if (value != null)
                return value;
            if (fallbackValue != null)
                return fallbackValue;

            return defaultValue;
        }
    }

}
