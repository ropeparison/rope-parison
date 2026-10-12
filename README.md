# RopeParison

https://ropeparison.azurewebsites.net/  
  
A project to gather a database of all climbing ropes and display the data in a user friendly way for climbers to look at when considering getting a new rope.  
  
Secondary objectives:  
Present user friendly information about what the tests in the 'standards' actually look like.  
Examine the rope data to identify any interesting correlations and/or dispell misconceptions climbers have about rope properties.  
Look into using the data from triple rated ropes to create a spring-damper model of a rope.  
Expand to include static ropes and cords.
  
Stack:  
ASP.NET Core Blazor Server  
Entity Framework Core  
SQL Server  
Syncfusion Blazor (thanks to Syncfusion for their community licence)  

## Development

The solution targets .NET 10. See [upgrade and local setup notes](reviews/DOTNET10_UPGRADE.md) and [regression test instructions](RopeParison.Tests/README.md). The original failing cases are fixed. See [defect fixes and remaining work](reviews/DEFECT_FIXES.md). SQL Server integration tests are opt-in; test instructions describe the isolated database setup.
