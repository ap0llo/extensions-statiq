# Statiq Extensions

Extension/custom modules for [Statiq.Framework](https://statiq.dev/framework/)

⚠ Very much a work-in-progress

## Building from source

```bat
  dotnet restore .\src\Extensions.Statiq.sln

  dotnet build .\src\Extensions.Statiq.sln

  dotnet pack .\src\Extensions.Statiq.sln
```

## Acknowledgments

Built on [Statiq.Framework](https://statiq.dev/framework/)

## Versioning and Branching

The version of this library is automatically derived from git and the information
in `version.json` using [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning):

- The master branch  always contains the latest version. Packages produced from
  master are always marked as pre-release versions (using the `-pre` suffix).
- Stable versions are built from release branches. Build from release branches
  will have no `-pre` suffix
- Builds from any other branch will have both the `-pre` prerelease tag and the git
  commit hash included in the version string

To create a new release branch use the [`nbgv` tool](https://www.nuget.org/packages/nbgv/)
(at least version `3.0.4-beta`):

```ps1
dotnet tool install --global nbgv --version 3.0.4-beta
nbgv prepare-release
```
