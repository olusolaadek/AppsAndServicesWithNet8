# Project
Entity Models for Northwind Database in SQL Server

# Description
This project contains the entity models for the Northwind database, which is a sample database provided by Microsoft. The models are designed to be used with Entity Framework Core and are compatible with SQL Server.

## Scalfolding
dotnet ef dbcontext scaffold "Data Source=.;Initial Catalog=Northwind;Integrated Security=true;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --namespace Northwind.EntityModels --data-annotations

