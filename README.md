# VisualStudioSolutionRemoveStaleDependencies
Utility to Remove "Stale" Projects from Visual Studio Solutions

"Stale" Projects within a Visual Studio Solution file are defined as one or more of the following:

1. A Project that is included in the solution file but does not exist on disk
2. A Project that was included in a "Dependencies" Folder but now no longer exists

In general this is used to remove projects which straight up no longer exist on disk.

This should not be confused with https://github.com/aolszowka/VisualStudioSolutionShaker which while similar sounding, attempts to perform the "shaking" of valid (projects that exist on disk) "Dependencies" projects that are not actual N-Order dependencies.
