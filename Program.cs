using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);
var employees = new List<Employee>
{
    new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
    new Employee { Id = 2, FirstName = "Jane", LastName = "Smith" }
};

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/employees", () =>
{
    return Results.Ok(employees);
});

app.MapGet("/employees/{id}", (int id) =>
{
    var employee = employees.SingleOrDefault(e => e.Id == id);
    bool employeeNotFound = employee == null;

    if (employeeNotFound)
    {
        return Results.NotFound();
    }

    return Results.Ok(employee);
});

app.Run();