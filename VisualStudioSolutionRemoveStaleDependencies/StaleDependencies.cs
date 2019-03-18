// -----------------------------------------------------------------------
// <copyright file="StaleDependencies.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class StaleDependencies
    {
        public static IEnumerable<string> IdentifyStaleDependencies( string targetSolution )
        {
            // First Grab a list of all projects in the Dependencies Folder
            ISet<string> dependenciesProjects = new HashSet<string>(SolutionUtilities.GetDependenciesProjects(targetSolution), StringComparer.InvariantCultureIgnoreCase);

            // Now Grab a list of all projects in the solution
            ISet<string> projectsInSolution = new HashSet<string>(SolutionUtilities.GetProjectsFromSolutionFullPath(targetSolution), StringComparer.InvariantCultureIgnoreCase);

            // Remove any Projects that were in the Dependencies Folder
            IEnumerable<string> projectsInSolutionThatAreNotDependencies = projectsInSolution.Except(dependenciesProjects);

            // Now rescan for dependencies
            IEnumerable<string> currentNOrderDependencies = MSBuildUtilities.ProjectsIncludingNOrderDependencies(projectsInSolutionThatAreNotDependencies);

            IEnumerable<string> result = projectsInSolution.Except(currentNOrderDependencies);

            return result;
        }

        internal static IEnumerable<string> GetGuidsForProjects( string slnFile, IEnumerable<string> missingProjects, ConcurrentDictionary<string, string> missingGuidLookup )
        {
            // The Missing GUID Lookup Dictionary has the paths as fully
            // qualified; but in order to assist in searching the
            // missingProjects are relative to the solution; therefore
            // we need to expand it prior to looking up the Guid.
            string solutionDirectory = Path.GetDirectoryName(slnFile);

            foreach(string missingProjectRelativePath in missingProjects)
            {
                // Expand the Relative Project Path
                string missingProjectFullPath = PathUtilities.ResolveRelativePath(solutionDirectory, missingProjectRelativePath);

                // See if we get a dictionary hit to avoid the cost of lookups
                if(!missingGuidLookup.ContainsKey(missingProjectFullPath))
                {
                    // We had a cache miss; perform the expensive lookup task
                    // First find the line that contains the project reference
                    string projectLine = File.ReadAllLines(slnFile).Where(currentLine => currentLine.Contains(missingProjectRelativePath)).FirstOrDefault();

                    if(projectLine == null)
                    {
                        throw new InvalidOperationException($"Should have had at least one line containing `{missingProjectRelativePath}` in solution `{slnFile}`");
                    }

                    // Use a Regex to extract the Guid from the project
                    string extractedProjectGuid = Regex.Match(projectLine, ".*[ \\t]*=[ \\t]*.*,.*,[ \\t]*\"({[A-Za-z0-9-]+})\"").Groups[1].Value;

                    if(string.IsNullOrEmpty(extractedProjectGuid))
                    {
                        string exceptionMessage = $"Failed to extract GUID from `{slnFile}` for `{projectLine}`";
                        throw new InvalidOperationException(exceptionMessage);
                    }

                    // Now add it to the lookup dictionary
                    missingGuidLookup.TryAdd(missingProjectFullPath, extractedProjectGuid);
                }

                yield return missingGuidLookup[missingProjectFullPath];
            }
        }

        internal static IEnumerable<string> IdentifyMissingDependencies( string slnFile )
        {
            IEnumerable<string> allProjectsInSolution = SolutionUtilities.GetProjectsFromSolutionRelative(slnFile);
            string solutionDirectory = Path.GetDirectoryName(slnFile);

            foreach(string projectInSolution in allProjectsInSolution)
            {
                // Expand the path
                string fullPath = PathUtilities.ResolveRelativePath(solutionDirectory, projectInSolution);

                if(!File.Exists(fullPath))
                {
                    yield return projectInSolution;
                }
            }
        }
    }
}
