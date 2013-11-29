param($installPath, $toolsPath, $package, $project)

# Get the open solution.
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

# Create the parent solution folder.
$parentProject = $solution.AddSolutionFolder(".nploy")