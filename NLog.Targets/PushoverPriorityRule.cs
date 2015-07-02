using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using NPushover.RequestObjects;
using System.ComponentModel;

namespace NLog.Targets
{
    [NLogConfigurationItem]
    public class PushoverPriorityRule
    {
        static PushoverPriorityRule()
        {
            Default = new PushoverPriorityRule();
        }

        public PushoverPriorityRule()
            : this(null, Priority.Normal) { }

        public PushoverPriorityRule(ConditionExpression condition, Priority priority)
            : this(condition, priority, Sounds.Pushover) { }

        public PushoverPriorityRule(ConditionExpression condition, Priority priority, Sounds sound)
            : this(condition, priority, sound, 0, 0) { }

        public PushoverPriorityRule(ConditionExpression condition, Priority priority, Sounds sound, int retryIntervalSeconds, int retryPeriodSeconds)
        {
            this.Condition = condition;
            this.Priority = priority;
            this.Sound = sound;
            this.RetryIntervalSeconds = retryIntervalSeconds;
            this.RetryPeriodSeconds = retryPeriodSeconds;
        }

        public static PushoverPriorityRule Default { get; private set; }

        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        [DefaultValue("Normal")]
        public Priority Priority { get; set; }

        [DefaultValue("Pushover")]
        public Sounds Sound { get; set; }

        [DefaultValue(0)]
        public int RetryIntervalSeconds { get; set; }

        [DefaultValue(0)]
        public int RetryPeriodSeconds { get; set; }
        public string RetryCallbackURL { get; set; }

        public string SuplementaryURL { get; set; }
        public string SuplementaryURLTitle { get; set; }

        public Layout UserOrGroupName { get; set; }
        public Layout DeviceName { get; set; }
        public Layout Subject { get; set; }

        public bool CheckCondition(LogEventInfo logEvent)
        {
            if (this.Condition == null)
            {
                return true;
            }

            return true.Equals(this.Condition.Evaluate(logEvent));
        }
    }
}
