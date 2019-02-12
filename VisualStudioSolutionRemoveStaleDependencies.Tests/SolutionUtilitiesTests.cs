// -----------------------------------------------------------------------
// <copyright file="SolutionUtilitiesTests.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies.Tests
{
    using NUnit.Framework;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class SolutionUtilitiesTests
    {
        static string TEST_ROOT_DIRECTORY = TestContext.CurrentContext.TestDirectory;

        [Test]
        public void GetNestedProjectsForGuid_ValidArguments()
        {
            string targetSolution = Path.Combine(TEST_ROOT_DIRECTORY, @"SolutionReferenceTester\SolutionReferenceTester.sln");
            string targetDependenciesGuid = @"{8175C2B3-EDF4-41E5-9A52-564CE9E84324}";
            string[] expectedResult =
                new string[]
                {
                    "{4ACCAD4A-949C-4DFC-A449-B288BB5239C6}",
                    "{7B0E316A-3C60-4166-B59B-338CBB2DFD0C}",
                    "{FA426B0F-A3B1-4FCA-91B0-74100460E464}",
                };

            string[] actual = SolutionUtilities.GetNestedProjectsForGuid(targetSolution, targetDependenciesGuid).ToArray();

            Assert.That(actual, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetDependenciesProjects_ValidArguments()
        {
            string targetSolution = Path.Combine(TEST_ROOT_DIRECTORY, @"SolutionReferenceTester\SolutionReferenceTester.sln");
            string[] expected =
                new string[]
                {
                    Path.Combine(TEST_ROOT_DIRECTORY,@"SolutionReferenceTester\A\A.csproj"),
                    Path.Combine(TEST_ROOT_DIRECTORY,@"SolutionReferenceTester\B\B.csproj"),
                    Path.Combine(TEST_ROOT_DIRECTORY,@"SolutionReferenceTester\C\C.csproj"),
                };

            string[] actual = SolutionUtilities.GetDependenciesProjects(targetSolution).ToArray();

            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
