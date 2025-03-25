using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace TheEmployeeApi.Employees.Validators
{
    public class GetAllEmployeesRequestValidator : AbstractValidator<GetAllEmployeesRequest>
    {
        public GetAllEmployeesRequestValidator()
        {
            RuleFor(r => r.Page).GreaterThanOrEqualTo(1).WithMessage("Page number must be set to a positive non-zero integer.");
            RuleFor(r => r.RecordsPerPage)
                .GreaterThanOrEqualTo(1).WithMessage("You must return at least one record.")
                .LessThanOrEqualTo(100).WithMessage("You cannot return more than 100 records.");
        }
    }
}