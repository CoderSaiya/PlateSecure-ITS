﻿using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlateSecure.Application.DTOs;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Lọc ra các parameter kiểu IFormFile hoặc IEnumerable<IFormFile>
        var fileParams = context.MethodInfo
            .GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile)
                        || p.ParameterType == typeof(IEnumerable<IFormFile>))
            .ToList();

        if (!fileParams.Any())
            return;

        // Đánh dấu request body là multipart/form-data
        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParams.ToDictionary(
                            p => p.Name,
                            p => new OpenApiSchema 
                            { 
                                Type = "string", 
                                Format = "binary" 
                            }
                        ),
                        Required = fileParams.Select(p => p.Name).ToHashSet()
                    }
                }
            }
        };
    }
}