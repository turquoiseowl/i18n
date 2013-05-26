using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NUnit.Framework;
using i18n.MSBuild.Task;
using i18n.Tests.Helpers;

namespace i18n.Tests
{
    [TestFixture]
    class MSBuildTests : MockSessionProvider
    {

        protected static string Cwd = AppDomain.CurrentDomain.BaseDirectory;
        protected static string ThisProjectDirectory = Path.GetFullPath(Path.Combine(Cwd, @"..\..\"));
        protected static string MsBuildCsProject = Path.GetFullPath(
            Path.Combine(Cwd, @"..\..\i18n.MSBuild.Task\i18n.MSBuild.Task.csproject"));

        private Mock<IBuildEngine> _engine;

        [SetUp]
        public void SetUp()
        {
            Initialize();
            _engine = new Mock<IBuildEngine>();
            _engine.Setup(engine => engine.ProjectFileOfTaskNode).Returns(MsBuildCsProject);
        }

        [Test]
        public void CanExtractTranslationsUsingMsbuild()
        {

            // NB! This task extracts all gettext strings from this project.
            // And is used later on to verify that everything worked as expected.

            // Arrange
            var projectDirectories = new ITaskItem[]
                                     {
                                         new TaskItem(ThisProjectDirectory)
                                     };

            // Act
            var success = new i18nTask
                       {
                           BuildEngine = _engine.Object,
                           ProjectDirectories = projectDirectories,
                           OutputPath = Cwd,
                       }.Execute();

            // Assert
            Assert.True(success);

        }

        [Test]
        public void VerifyMessagePoExistsForDefaultLanguage()
        {
            Assert.True(File.Exists(Path.Combine(Cwd, @"locale\{0}\messages.po".FormatWith(DefaultLanguage))));
        }

        [Test, Ignore("FIXME: Add test that ensures that messages.pot has valid content.")]
        public void VerifyContentsOfMessagesPot()
        {
        }

    }
}
