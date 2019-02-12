// -----------------------------------------------------------------------
// <copyright file="StaleDependencies.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StaleDependencies
    {
        public static IEnumerable<string> IdentifyStaleDependencies( string targetSolution )
        {
            // First Grab a list of all projects in the Dependencies Folder
            ISet<string> dependenciesProjects = new HashSet<string>(SolutionUtilities.GetDependenciesProjects(targetSolution), StringComparer.InvariantCultureIgnoreCase);

            // Now Grab a list of all projects in the solution
            ISet<string> projectsInSolution = new HashSet<string>(SolutionUtilities.GetProjectsFromSolution(targetSolution), StringComparer.InvariantCultureIgnoreCase);

            // Remove any Projects that were in the Dependencies Folder
            IEnumerable<string> projectsInSolutionThatAreNotDependencies = projectsInSolution.Except(dependenciesProjects);

            // Now rescan for dependencies
            IEnumerable<string> currentNOrderDependencies = MSBuildUtilities.ProjectsIncludingNOrderDependencies(projectsInSolutionThatAreNotDependencies);

            IEnumerable<string> result = projectsInSolution.Except(currentNOrderDependencies);

            return result;
        }
    }
}
