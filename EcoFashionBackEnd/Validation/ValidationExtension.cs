//using Microsoft.AspNetCore.Mvc;
//using FluentValidation.Results;
//using System.Runtime.CompilerServices;

//namespace EcoFashionBackEnd.Validation
//{
//    public static class ValidationExtension
//    {
//        public static ValidationProblemDetails  ToProblemDetails(this ValidationResult result)
//        {
//            // init validation problems details
//            var error = new ValidationProblemDetails
//            {
//                Status = 400
//            };

//            // each error in ValidationResult.Errors is ValidationFailure
//            // -> contain Property, ErrorMessage
//            foreach (var validationFailure in result.Errors)
//            {
//                // if error property already exist
//                if (error.Errors.ContainsKey(validationFailure.PropertyName))
//                {
//                    // from key -> get value and concat with errors arr 
//                    error.Errors[validationFailure.PropertyName] =
//                        error.Errors[validationFailure.PropertyName]
//                            .Concat(new[] { validationFailure.ErrorMessage }).ToArray();
//                }

//                error.Errors.Add(new KeyValuePair<string, string[]>(
//                    validationFailure.PropertyName,
//                    new[] { validationFailure.ErrorMessage }));
//            }

//            return error;
//        }

//    }
//}
