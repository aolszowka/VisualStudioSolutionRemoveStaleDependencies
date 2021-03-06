﻿// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Ace Olszowka">
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
    using System.Threading.Tasks;

    class Program
    {
        static void Main( string[] args )
        {
            string targetDirectory = @"";
            RemoveMissingDependenciesFromSolutions(targetDirectory);
            ModifyStaleSolutions(targetDirectory);
        }

        static void RemoveMissingDependenciesFromSolutions( string targetDirectory )
        {
            IEnumerable<string> slnsInDirectory = Directory.EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories);
            ConcurrentDictionary<string, string> missingGuidLookup = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            //Parallel.ForEach(slnsInDirectory, slnFile =>
            foreach(string slnFile in slnsInDirectory)
            {
                string[] staleProjects = StaleDependencies.IdentifyMissingDependencies(slnFile).ToArray();

                if(staleProjects.Any())
                {
                    // Because the dependency is missing we must parse the project for the project Guid
                    string[] guidsToRemove = StaleDependencies.GetGuidsForProjects(slnFile, staleProjects, missingGuidLookup).ToArray();
                    Console.WriteLine($"Modifying {slnFile}");
                    SolutionModificationUtilities.RemoveProjectsByGuidFromSolution(slnFile, guidsToRemove);
                }
            }
            //);
        }

        static void ModifyStaleSolutions( string targetDirectory )
        {
            IEnumerable<string> slnsInDirectory = Directory.EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories);

            Parallel.ForEach(slnsInDirectory, slnFile =>
            {
                string[] staleProjects = StaleDependencies.IdentifyStaleDependencies(slnFile).ToArray();

                if(staleProjects.Any())
                {
                    Console.WriteLine($"Modifying {slnFile}");
                    SolutionModificationUtilities.RemoveProjectsFromSolution(slnFile, staleProjects);
                }
            }
            );
        }

        static void IdentifyStaleSolutions( string targetDirectory )
        {
            ConcurrentDictionary<string, string[]> solutionsWithStaleProjects = new ConcurrentDictionary<string, string[]>();
            IEnumerable<string> slnsInDirectory = Directory.EnumerateFiles(targetDirectory, "*.sln", SearchOption.AllDirectories);

            Parallel.ForEach(slnsInDirectory, slnFile =>
            {
                string[] staleProjectsInSolution = StaleDependencies.IdentifyStaleDependencies(slnFile).ToArray();

                if(staleProjectsInSolution.Any())
                {
                    solutionsWithStaleProjects.TryAdd(slnFile, staleProjectsInSolution);
                }
            }
            );

            foreach(KeyValuePair<string, string[]> solutionWithStaleProjects in solutionsWithStaleProjects.OrderBy(kvp => kvp.Key))
            {
                Console.WriteLine($"== {solutionWithStaleProjects.Key} {solutionWithStaleProjects.Value.Length} == ");
                foreach(string result in solutionWithStaleProjects.Value)
                {
                    Console.WriteLine(result);
                }
                Console.WriteLine();
            }
        }
    }
}
