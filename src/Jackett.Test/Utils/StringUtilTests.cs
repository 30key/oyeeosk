using System.Collections.Specialized;
using NUnit.Framework;

namespace Jackett.Common.Utils.Tests
{
    [TestFixture]
    public class StringUtilTests
    {
        [Test]
        public void GetQueryStringTests()
        {
            var testCase = new NameValueCollection
            {
                { "q", "test" },
                { "st", "troy" },
                { "makin", "thisup" },
                { "st", "duplicate" }
            };
            const string combinedTemplate = "q=test{0}st=troy%2Cduplicate{0}makin=thisup";
            const string splitTemplate = "q=test{0}st=troy{0}st=duplicate{0}makin=thisup";
            Assert.AreEqual(string.Format(combinedTemplate, "&"), testCase.GetQueryString());
            Assert.AreEqual(string.Format(combinedTemplate, ";"), testCase.GetQueryString(separator: ";"));
            Assert.AreEqual(string.Format(splitTemplate, "&"), testCase.GetQueryString(duplicateKeysIfMulti: true));
            Assert.AreEqual(
                string.Format(splitTemplate, ";"), testCase.GetQueryString(duplicateKeysIfMulti: true, separator: ";"));
        }
    }
}
