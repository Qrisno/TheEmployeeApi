using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TheEmployeeApi.Employees;

namespace TheEmployeeAPI.Tests;

public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        var repo = _factory.Services.GetRequiredService<IRepository<Employee>>();

        repo.Create(new Employee
        {
            Id = 1,
            SocialSecurityNumber = "234",
            FirstName = "John",
            LastName = "Doe",
            Benefits = new List<EmployeeBenefits>
            {
                new EmployeeBenefits
                {
                    Id = 12,
                    EmployeeId = 1,
                    BenefitType =  BenefitType.Health,
                    Cost = 12.4M
                },
                new EmployeeBenefits
                {
                    Id = 122,
                    EmployeeId = 1,
                    BenefitType =  BenefitType.Dental,
                    Cost = 12.4M
                }
            }
        });
    }

    [Fact]
    public async Task GetEmployees_ReturnOkStatus()
    {
        HttpClient client = _factory.CreateClient();

        var response = await client.GetAsync("/employees");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEmployeeById_ReturnOkStatus()
    {
        HttpClient client = _factory.CreateClient();

        var response = await client.GetAsync("/employees/1");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEmployeeById_ReturnNotFound()
    {
        HttpClient client = _factory.CreateClient();
        var response = await client.GetAsync("/employees/3870");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddEmployee_ReturnCreated()
    {
        HttpClient client = _factory.CreateClient();
        var employee = new CreateEmployeeRequest { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123" };
        var response = await client.PostAsJsonAsync("employees", employee);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddEmployee_ReturnBadRequest()
    {
        HttpClient client = _factory.CreateClient();
        var invalid_employee = new { };

        var response = await client.PostAsJsonAsync("employees", invalid_employee);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnOk()
    {
        HttpClient client = _factory.CreateClient();
        var employee = new UpdateEmployeeRequest { Id = 1, Address1 = "Jane Street" };

        var response = await client.PutAsJsonAsync("employees", employee);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnNotFound()
    {
        HttpClient client = _factory.CreateClient();
        var employee = new UpdateEmployeeRequest { Id = 3870, Address1 = "Jano" };

        var response = await client.PutAsJsonAsync("employees", employee);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_ReturnsBadRequest()
    {
        HttpClient client = _factory.CreateClient();
        var emptyEmployee = new CreateEmployeeRequest();

        var response = await client.PostAsJsonAsync("employees", emptyEmployee);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("FirstName", problemDetails!.Errors.Keys);
        Assert.Contains("LastName", problemDetails!.Errors.Keys);
        Assert.Contains("'First Name' must not be empty.", problemDetails!.Errors["FirstName"]);
        Assert.Contains("'Last Name' must not be empty.", problemDetails!.Errors["LastName"]);
    }

    [Fact]
    public async Task GetEmployeeBenefits_ReturnsNotFound()
    {
        HttpClient client = _factory.CreateClient();
        var res = await client.GetAsync("employees/0/Benefits");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeBenefits_ReturnsOk()
    {
        HttpClient client = _factory.CreateClient();
        var res = await client.GetAsync("employees/1/Benefits");
        res.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEmployeeBenefits_ReturnsTwoBenefits()
    {
        HttpClient client = _factory.CreateClient();
        var res = await client.GetAsync("employees/1/Benefits");
        var benefits = await res.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponseEmployeeBenefit>>();
        Assert.Equal(2, benefits.Count());
    }
}