# Project

## Installation Required

Make sure you have the following installed on your system:

As this project will be making use of the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0),
ensure that you have it installed and properly configured on your machine.
You can verify the installation by running the following command in your terminal:

```bash
dotnet --version
```

You should then see the version number of the .NET SDK installed on your system.

E.g: `10.x.xxx`

## Packages & Installation

Packages used during the development of this project are listed below.

```bash
# Used for creating a beautiful console application.
dotnet add package Spectre.Console

# Used for logging to console.
dotnet add package Microsoft.Extensions.Logging

# Used for dependency injection.
dotnet add package Microsoft.Extensions.DependencyInjection

# Used for configuration management.
dotnet add package Microsoft.Extensions.Configuration

# Used for configuration management, specifically json files.
dotnet add package Microsoft.Extensions.Configuration.Json 

# Used for adding extensions on base method of Spectre.Console
dotnet add package Spectre.Console.Extensions

# Used for formatting C# code.
dotnet add package CSharpier.MSBuild
```

**Link**

- [Spectre.Console](https://www.nuget.org/packages/Spectre.Console/0.57.3-alpha.0.7)
- [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/11.0.0-preview.6.26359.118)
- [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/11.0.0-preview.6.26359.118)
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/11.0.0-preview.6.26359.118)
- [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/11.0.0-preview.6.26359.118)
- [Spectre.Console.Extensions](https://www.nuget.org/packages/Spectre.Console.Extensions)
- [CSharpier.MSBuild](https://www.nuget.org/packages/CSharpier.MsBuild/)
