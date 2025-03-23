using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheEmployeeApi.Employees;

public class EmployeeBenefits
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public BenefitType BenefitType { get; set; }
    public decimal Cost { get; set; }

    public Employee Employee { get; set; } = null!;
}

public enum BenefitType
{
    Health,
    Dental,
    Vision
}

