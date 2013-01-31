using NUnit.Framework;
using i18n.PostBuild;

namespace i18n.Tests
{
    [TestFixture]
    public class PostBuildTaskTests
    {
        [Test]
        public void Can_process_message_template()
        {
            const string path = @"TEST_PROJECT_DIR";
            var task = new PostBuildTask();
            task.Execute(path);
        }
    }
}
