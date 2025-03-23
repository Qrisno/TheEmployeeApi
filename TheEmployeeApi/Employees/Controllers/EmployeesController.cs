using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheEmployeeApi;
using TheEmployeeApi.Employees;// Assuming models like CreateEmployeeRequest, UpdateEmployeeRequest, etc., are here
using System.Collections.Generic;
using System.Threading.Tasks;

public class EmployeesController : BaseController
{
    public readonly IRepository<Employee> _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IRepository<Employee> repository, ILogger<EmployeesController> logger)
    {
        _repository = repository;
        _logger = logger;

    }

    /// <summary>
    /// Returns All existing employees
    /// </summary>
    /// <returns>All Employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAll()
    {
        var employees = _repository.GetAll().Select(employee => ConvertEmployeeToGetEmployeeResponse(employee));
        return Ok(employees);
    }

    /// <summary>
    /// Returns Individual Employee Data
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Employee</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        return Ok(ConvertEmployeeToGetEmployeeResponse(employee));
    }

    [HttpGet("{id:int}/Benefits")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetEmployeeBenefits([FromRoute] int id)
    {
        var employee = _repository.GetById(id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee.Benefits.Select(benefit => new GetEmployeeResponseEmployeeBenefit
        {
            Id = benefit.Id,
            EmployeeId = benefit.EmployeeId,
            BenefitType = benefit.BenefitType,
            Cost = benefit.Cost
        }));
    }


    /// <summary>
    /// Updates Employee Data
    /// </summary>
    /// <param name="employee"></param>
    /// <returns>Updated Employee</returns>
    [HttpPut]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateEmployee(UpdateEmployeeRequest employee)
    {
        _logger.LogInformation($"Trying to update Employee with ID:  {employee.Id}");
        var existingEmployee = _repository.GetById(employee.Id);

        if (existingEmployee == null)
        {
            _logger.LogWarning($"Trying to update Employee with ID:  {employee.Id}, Employee Id not found !");
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

    /// <summary>
    /// Adds New Employee
    /// </summary>
    /// <param name="employee"></param>
    /// <returns>Nothing</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            Email = employee.Email,
            Benefits = employee.Benefits
        };
        _repository.Create(newEmployee);
        return Created();
    }

    private GetEmployeeResponse ConvertEmployeeToGetEmployeeResponse(Employee employee)
    {
        return new GetEmployeeResponse
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email,
            Benefits = employee.Benefits.Select(benefit => new GetEmployeeResponseEmployeeBenefit
            {
                Id = benefit.Id,
                EmployeeId = benefit.EmployeeId,
                BenefitType = benefit.BenefitType,
                Cost = benefit.Cost
            }).ToList()
        };
    }
}