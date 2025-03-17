using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TheEmployeeApi.Employees;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRepository<Employee>, EmployeeRepository>();
builder.Services.AddProblemDetails();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var employeeRoute = app.MapGroup("/employees");
employeeRoute.MapGet(string.Empty, (IRepository<Employee> repo) =>
{
    var employees = repo.GetAll();
    return Results.Ok(employees.Select(employee => new GetEmployeeResponse
    {
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    }));
});

employeeRoute.MapGet("{id:int}", (int id, IRepository<Employee> repo) =>
{
    var employee = repo.GetById(id);


    if (employee == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new GetEmployeeResponse
    {
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    });
});

employeeRoute.MapPost("AddEmployee", (CreateEmployeeRequest employee, IRepository<Employee> repo) =>
{
    var validationProblems = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(employee, new ValidationContext(employee), validationProblems, true);

    if (!isValid)
    {
        return Results.BadRequest(validationProblems);
    }
    var newEmployee = new Employee
    {
        FirstName = employee.FirstName!,
        LastName = employee.LastName!,
        SocialSecurityNumber = employee.SocialSecurityNumber,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    };
    repo.Create(newEmployee);
    return Results.Created();

});

employeeRoute.MapPut("UpdateEmployee", (UpdateEmployeeRequest employee, IRepository<Employee> repo) =>
{
    var existingEmployee = repo.GetById(employee.Id);

    if (existingEmployee == null)
    {
        return Results.NotFound();
    }

    existingEmployee.Address1 = employee.Address1;
    existingEmployee.Address2 = employee.Address2;
    existingEmployee.City = employee.City;
    existingEmployee.State = employee.State;
    existingEmployee.ZipCode = employee.ZipCode;
    existingEmployee.PhoneNumber = employee.PhoneNumber;
    existingEmployee.Email = employee.Email;
    repo.Update(existingEmployee);
    return Results.Ok(existingEmployee);
});

app.Run();

public partial class Program
{

}