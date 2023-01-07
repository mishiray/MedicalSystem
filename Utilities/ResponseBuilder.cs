using MedicalSystem.DTOs;
using MedicalSystem.DTOs.Enums;
using MedicalSystem.DTOs.ServiceDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MedicalSystem.Utilities
{
    public static class ResponseBuilder
    {
        public static GlobalResponse<T> BuildResponse<T>(ModelStateDictionary errs, T data)
        {
            var listOfErrorItems = new List<ErrorItemModel>();
            var benchMark = new List<string>();

            if (errs != null)
            {
                foreach (var err in errs)
                {
                    ///err.error.errors
                    var key = err.Key;
                    var errValues = err.Value;
                    var errList = new List<string>();
                    foreach (var errItem in errValues.Errors)
                    {
                        errList.Add(errItem.ErrorMessage);
                        if (!benchMark.Contains(key))
                        {
                            listOfErrorItems.Add(new ErrorItemModel { Key = key, ErrorMessages = errList });
                            benchMark.Add(key);
                        }
                    }
                }
            }

            var response = new GlobalResponse<T>
            {
                Data = data,
                Errors = listOfErrorItems,
            };

            return response;
        }
    }
}
