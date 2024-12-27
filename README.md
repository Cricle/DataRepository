<h1 align="center">
DataRespository
</h1>

<h6 align="center">
A small, easily, respository mode implement for .NET
</h6>

<div align="center">

[![.NET Build](https://github.com/Cricle/DataRepository/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Cricle/DataRepository/actions/workflows/dotnet.yml)
[![.NUGET Push](https://github.com/Cricle/DataRepository/actions/workflows/nuget.yml/badge.svg)](https://github.com/Cricle/DataRepository/actions/workflows/nuget.yml)
[![codecov](https://codecov.io/gh/Cricle/DataRepository/graph/badge.svg?token=XOmsOPqYJU)](https://codecov.io/gh/Cricle/DataRepository)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/fdb99efc07604de9b23709cd19e2d863)](https://app.codacy.com/gh/Cricle/DataRepository/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

</div>

## How to use

### Install the [DataRepository.EFCore.DependencyInjection](https://www.nuget.org/packages/DDataRepository.EFCore.DependencyInjection)

```powershell
dotnet add package DDataRepository.EFCore.DependencyInjection --version 1.0.3
```

### Add service in DI

```csharp
builder.Services.AddRespository<{Your DbContext}>();
```

### Add in the respository interface

```csharp

public class NumberService(IDataRespository<Number> respository)
{
    public async Task<IWorkPageResult<Number>> PageAsync(int pageIndex, int pageSize) => await respository.PageQueryAsync(pageIndex, pageSize);

    // To insert data
    public async Task<WorkDataResult<bool>> InsertAsync(int id, int value)
        => await respository.InsertAsync(new Number { Id = id, Value = value }) > 0;

    // To update data
    public async Task<WorkDataResult<bool>> UpdateAsync(int id, int value)
        => await respository.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, value)) > 0;

    // To update data ref database column
    public async Task<WorkDataResult<bool>> IncreaseAsync(int id, int value)
        => await respository.Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, y => y.Value + value)) > 0;

    // To delete data
    public async Task<WorkDataResult<bool>> DeleteAsync(int id)
        => await respository.Where(x => x.Id == id).ExecuteDeleteAsync() > 0;
}

```

NOTE: `ExecuteUpdateAsync`, `ExecuteUpdate`, `ExecuteDeleteAsync`, `ExecuteDelete` same as EFCore 8.0 Bulk update and delete, if the project use EFCore 8.0 or later, it would be used from EFCore raw method, otherwise use this class extensions methods

### And you can use like DbContext

```csharp
IDataRespositoryScope scope = ...;//From DI

var students = scope.Create<Student>();//Return IDataRespository<Student>
var teachers = scope.Create<Teacher>();//Return IDataRespository<Teacher>

```
