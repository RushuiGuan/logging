* 10.0.0 - Upgrading dependencies from dotnet 9 to 10
* 9.0.0 - Upgrading dependencies from dotnet 8 to 9
* 8.0.0 - Remove reference to `Serilog.Sinks.Email` since it contains a vulnerability and it is not used by the package.  It is technically a breaking change since the consumer will need to reference the package directly if they want to use it.  Therefore bumping the major version.
