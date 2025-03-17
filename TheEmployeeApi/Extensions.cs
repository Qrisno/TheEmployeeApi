using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace TheEmployeeApi;

public static class Extensions
{
    public static ValidationProblemDetails ToValidationProblemDetails(this List<ValidationResult> validationResults)
    {
        var problemDetails = new ValidationProblemDetails();



        foreach (var result in validationResults)
        {

            foreach (var memberName in result.MemberNames)
            {
                if (problemDetails.Errors.ContainsKey(memberName))
                {
                    problemDetails.Errors[memberName] = problemDetails.Errors[memberName].Concat([result.ErrorMessage]).ToArray()!;
                }
                else
                {
                    problemDetails.Errors[memberName] = new List<string> { result.ErrorMessage! }.ToArray()!;
                }
            }
        }
        return problemDetails;
    }
}
