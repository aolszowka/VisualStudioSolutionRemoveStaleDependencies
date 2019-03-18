// -----------------------------------------------------------------------
// <copyright file="SolutionUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2018-2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies
{
    using Microsoft.Build.Construction;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static class SolutionUtilities
    {
        internal static bool TryGetDepedenciesFolderGuid( SolutionFile targetSolution, out string dependenciesFolderGuid )
        {
            bool dependenciesFolderFound = false;
            dependenciesFolderGuid = string.Empty;

            ProjectInSolution[] dependenciesFolders =
                targetSolution
                .ProjectsInOrder
                .Where(project => project.ProjectType == SolutionProjectType.SolutionFolder)
                .Where(projectSolutionFolder => projectSolutionFolder.ProjectName.Equals("Dependencies"))
                .ToArray();

            if(dependenciesFolders.Length == 1)
            {
                // Best case is a folder already exist with this project; return its Guid
                dependenciesFolderGuid = dependenciesFolders.First().ProjectGuid;
                dependenciesFolderFound = true;
            }
            else if(dependenciesFolders.Length > 1)
            {
                string message = $"There were {dependenciesFolders.Length} Dependencies Folders Found; This is unexpected";
                throw new InvalidOperationException(message);
            }
            else
            {
                dependenciesFolderGuid = "{DA34CE5D-031A-4C97-8DE8-A81F98C0288A}";
                dependenciesFolderFound = false;
            }

            return dependenciesFolderFound;
        }

        internal static bool TryGetDepedenciesFolderGuid( string targetSolution, out string dependenciesFolderGuid )
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            return TryGetDepedenciesFolderGuid(solution, out dependenciesFolderGuid);
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of the path of all of the referenced projects.
        /// </summary>
        /// <param name="targetSolution">The solution to evaluate.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the fully qualified path of all of the referenced projects.</returns>
        internal static IEnumerable<string> GetProjectsFromSolutionFullPath( string targetSolution )
        {
            string solutionFolder = Path.GetDirectoryName(targetSolution);

            return GetProjectsFromSolutionRelative(targetSolution)
            .Select(projectRelativePath => PathUtilities.ResolveRelativePath(solutionFolder, projectRelativePath));
        }

        internal static IEnumerable<string> GetProjectsFromSolutionRelative( string targetSolution )
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);

            return
                solution
                .ProjectsInOrder
                .Where(project => project.ProjectType != SolutionProjectType.SolutionFolder)
                .Select(project => project.RelativePath);
        }

        internal static IEnumerable<string> GetDependenciesProjects( string targetSolution )
        {
            SolutionFile solution = SolutionFile.Parse(targetSolution);
            string solutionFolder = Path.GetDirectoryName(targetSolution);
            string dependenciesFolderGuid = string.Empty;

            // First try to get the Dependencies Folder Guid
            if(TryGetDepedenciesFolderGuid(solution, out dependenciesFolderGuid))
            {
                IEnumerable<string> dependenciesProjectsGuids = GetNestedProjectsForGuid(targetSolution, dependenciesFolderGuid);

                foreach(string dependenciesProjectGuid in dependenciesProjectsGuids)
                {
                    string relativePathToProject = solution.ProjectsByGuid[dependenciesProjectGuid].RelativePath;
                    yield return PathUtilities.ResolveRelativePath(solutionFolder, relativePathToProject);
                }
            }
        }

        internal static IEnumerable<string> GetNestedProjectsForGuid( string targetSolution, string nestedProjectGuid )
        {
            // Right now this works by assuming that the nested projects
            // are in the format:
            //         { NESTED-PROJECT-GUID} = {FOLDER-TO-NEST-IN-GUID}
            // When this changes you'll need to fix this implementation
            return
                File
                .ReadLines(targetSolution)
                .Where(currentLine => currentLine.EndsWith(nestedProjectGuid, StringComparison.InvariantCultureIgnoreCase))
                .Select(currentLine => Regex.Match(currentLine.Trim(), @"^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?").Value);
        }
    }
}
