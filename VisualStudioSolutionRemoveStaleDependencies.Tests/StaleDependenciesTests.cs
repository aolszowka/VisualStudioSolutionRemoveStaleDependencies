// -----------------------------------------------------------------------
// <copyright file="StaleDependenciesTests.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies.Tests
{
    using NUnit.Framework;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class StaleDependenciesTests
    {
        static string TEST_ROOT_DIRECTORY = TestContext.CurrentContext.TestDirectory;

        [Test]
        public void StaleDependencies_Test()
        {
            string targetSolution = Path.Combine(TEST_ROOT_DIRECTORY, @"SolutionReferenceTester\SolutionReferenceTester.sln");
            string[] expectedStaleDirectories =
                new string[]
                {
                    Path.Combine(TEST_ROOT_DIRECTORY, @"SolutionReferenceTester\C\C.csproj"),
                };

            string[] actualStaleDependencies = StaleDependencies.IdentifyStaleDependencies(targetSolution).ToArray();

            Assert.That(actualStaleDependencies, Is.EquivalentTo(expectedStaleDirectories));
        }
    }
}
