// -----------------------------------------------------------------------
// <copyright file="SolutionModificationUtilities.cs" company="Ace Olszowka">
//  Copyright (c) Ace Olszowka 2019. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace VisualStudioSolutionRemoveStaleDependencies
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal static class SolutionModificationUtilities
    {
        internal static void RemoveProjectsFromSolution( string targetSolution, IEnumerable<string> projectsToRemove )
        {
            if(projectsToRemove.Any())
            {
                // For Each Project Grab the ProjectGuid
                string[] guidsOfProjectsToRemove = projectsToRemove.Select(projectToRemove => MSBuildUtilities.GetMSBuildProjectGuid(projectToRemove)).ToArray();

                // Now Open the file for reading
                IEnumerable<string> existingSolutionLines = File.ReadLines(targetSolution);

                // We are going to write the changes to a temporary file
                string tempFile = Path.GetTempFileName();

                // A 32kb Buffer seems to be about the best trade off
                using(StreamWriter sw = new StreamWriter(tempFile, false, new UTF8Encoding(false, true), 32768))
                {
                    bool skipNextLine = false;

                    foreach(string existingSolutionLine in existingSolutionLines)
                    {
                        if(skipNextLine)
                        {
                            skipNextLine = false;
                            continue;
                        }
                        else if(guidsOfProjectsToRemove.Any(guidToRemove => existingSolutionLine.Contains(guidToRemove)))
                        {
                            // Do not write out the line

                            // Also do not write out the next line if this
                            // line was a project line
                            if(existingSolutionLine.StartsWith("Project"))
                            {
                                skipNextLine = true;
                            }
                        }
                        else
                        {
                            // Write out the line
                            sw.WriteLine(existingSolutionLine);
                        }
                    }
                }

                // Finally overwrite the existing solution file with the temp
                File.Copy(tempFile, targetSolution, true);
                File.Delete(tempFile);
            }
        }
    }
}
