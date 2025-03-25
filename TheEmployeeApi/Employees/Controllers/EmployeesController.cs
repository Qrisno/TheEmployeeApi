using Microsoft.AspNetCore.Mvc;
using TheEmployeeApi;
using TheEmployeeApi.Employees;// Assuming models like CreateEmployeeRequest, UpdateEmployeeRequest, etc., are here
using Microsoft.EntityFrameworkCore;

public class EmployeesController : BaseController
{
    public readonly AppDbContext _dbContext;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext dbContext, ILogger<EmployeesController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

    }

    /// <summary>
    /// Returns All existing employees
    /// </summary>
    /// <returns>All Employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllEmployeesRequest? request)
    {
        int page = request?.Page ?? 1;
        int recordsPerPage = request?.RecordsPerPage ?? 100;
        IQueryable<Employee> query = _dbContext.Employees.Skip((page - 1) * recordsPerPage).Take(recordsPerPage);

        if (request != null)
        {
            if (!string.IsNullOrWhiteSpace(request.FirstNameContains))
            {
                query = query.Where(e => e.FirstName.Contains(request.FirstNameContains));
            }

            if (!string.IsNullOrWhiteSpace(request.LastNameContains))
            {
                query = query.Where(e => e.LastName.Contains(request.LastNameContains));
            }
        }
        var employees = await query.ToArrayAsync();
        return Ok(employees.Select(employee => ConvertEmployeeToGetEmployeeResponse(employee)));
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
    public async Task<IActionResult> GetById([FromRoute] string id)
    {
        if (!int.TryParse(id, out var employeeId))
        {
            return BadRequest("Invalid ID format.");
        }

        var employees = await _dbContext.Employees.ToArrayAsync();
        var employee = employees.SingleOrDefault(employee => employee.Id == int.Parse(id));


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
    public async Task<IActionResult> GetEmployeeBenefits([FromRoute] int id)
    {
        var employees = await _dbContext.Employees.ToArrayAsync();
        var employee = employees.SingleOrDefault(employee => employee.Id == id);

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
    public async Task<IActionResult> UpdateEmployee(UpdateEmployeeRequest employee)
    {
        _logger.LogInformation($"Trying to update Employee with ID:  {employee.Id}");
        var employees = await _dbContext.Employees.ToArrayAsync();
        var existingEmployee = employees.SingleOrDefault(e => employee.Id == e.Id);

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
        _dbContext.Employees.Update(existingEmployee);
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
        _dbContext.Employees.Add(newEmployee);
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