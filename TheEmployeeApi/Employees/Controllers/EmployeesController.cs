

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TheEmployeeApi;
using TheEmployeeApi.Employees;

public class EmployeesController : BaseController
{
    public readonly IRepository<Employee> _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IRepository<Employee> repository, ILogger<EmployeesController> logger)
    {
        _repository = repository;
        _logger = logger;

    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var employees = _repository.GetAll();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] string id)
    {
        if (!int.TryParse(id, out var employeeId))
        {
            return BadRequest("Invalid ID format.");
        }

        var employee = _repository.GetById(employeeId);


        if (employee == null)
        {
            return NotFound();
        }

        return Ok(new GetEmployeeResponse
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
    }



    [HttpPut]
    public IActionResult UpdateEmployee(UpdateEmployeeRequest employee)
    {
        _logger.LogInformation($"Trying to update Employee with ID:  {employee.Id}");
        var existingEmployee = _repository.GetById(employee.Id);

        if (existingEmployee == null)
        {
            return NotFound();
        }

        existingEmployee.Address1 = employee.Address1;
        existingEmployee.Address2 = employee.Address2;
        existingEmployee.City = employee.City;
        existingEmployee.State = employee.State;
        existingEmployee.ZipCode = employee.ZipCode;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Email = employee.Email;
        _repository.Update(existingEmployee);
        return Ok(existingEmployee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeRequest employee)
    {
        var validationResults = await ValidateAsync(employee);


        if (!validationResults.IsValid)
        {
            return ValidationProblem(validationResults.ToModelStateDictionary());
        }
        var newEmployee = new Employee
        {
            FirstName = employee.FirstName!,
            LastName = employee.LastName!,
            SocialSecurityNumber = employee.SocialSecurityNumber!,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email
        };
        _repository.Create(newEmployee);
        return Created();
    }
}