using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;


namespace NLog.TargetsTests
{
    [TestClass]
    public class PushoverTargetTests
    {
        private static PushoverTarget GetTarget(string configfile)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine("testconfigs", configfile + ".config"), true);
            return LogManager.Configuration.AllTargets.OfType<PushoverTarget>().First();
        }

        [TestMethod]
        public void Config_IsRead_Correctly()
        {
            var target = GetTarget("test");

            Assert.AreEqual(2, target.PriorityRules.Count);
            Assert.AreEqual("TESTtestTESTtestTESTtestTESTte", target.ApplicationKey);
            Assert.AreEqual("USERgroupUSERgroupUSERgroupUSE", target.UserOrGroupName.Render(null));
            Assert.AreEqual("Test", target.SuplementaryURLTitle);
            Assert.AreEqual("http://google.com", target.SuplementaryURL);
            Assert.AreEqual("Message from NLog on " + Environment.MachineName, target.Subject.Render(null));
            Assert.AreEqual("DEVICEdeviceDEVICEdeviceD", target.DeviceName.Render(null));
            Assert.AreEqual("us-ascii", target.Encoding);
            Assert.AreEqual(true, target.Html);
            Assert.AreEqual("pushover", target.Name);
            Assert.AreEqual(true, target.UseDefaultPriorityRules);
            Assert.AreEqual(2, target.PriorityRules.Count);

            var r1 = target.PriorityRules[0];
            Assert.AreEqual("ConditionRelationalExpression", r1.Condition.GetType().Name);
            Assert.IsNull(r1.DeviceName);
            Assert.AreEqual("UoGuOgUoGuOgUoGuOgUoGuOgUoGuOg", r1.UserOrGroupName.Render(null));

            Assert.AreEqual("http://examle.com", r1.SuplementaryURL);
            Assert.AreEqual("Foo bar", r1.SuplementaryURLTitle);
            Assert.IsNull(r1.RetryCallbackURL);

            var r2 = target.PriorityRules[1];
            Assert.AreEqual("PANIC!!", r2.Subject.Render(null));

            Assert.AreEqual(99, r2.RetryIntervalSeconds);
            Assert.AreEqual(666, r2.RetryPeriodSeconds);
            Assert.AreEqual("http://callback.org", r2.RetryCallbackURL);
        }

        [TestMethod]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void Throws_OnMissing_ApplicationKey()
        {
            var target = GetTarget("missing_appkey");
        }

        [TestMethod]
        [ExpectedException(typeof(NLogConfigurationException))]
        public void Throws_OnMissing_UserOrGroup()
        {
            var target = GetTarget("missing_userorgroup");
        }
    }
}
