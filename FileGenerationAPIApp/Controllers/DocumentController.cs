using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FileGenerationAPIApp.Wrappers;
using FileGenerationAPIApp.Models;
using System.Text.Json;
namespace FileGenerationAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly Services.IDocumentGeneratorService _docService;
        private readonly IWrapper _wrapper;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(Services.IDocumentGeneratorService docService, IWrapper wrapper, ILogger<DocumentController> logger)
        {
            _docService = docService;
            _wrapper = wrapper;
            _logger = logger;
        }        

        [HttpPost("generate")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Generate( [FromForm] DocumentRequest request, CancellationToken cancellationToken)
        {
            #region datasets and file Validate

            // Ensure we can read the raw form values in case model binding missed the posted values
            var template = request?.Template;
            if ((template == null || template.Length == 0) && Request?.Form?.Files != null && Request.Form.Files.Count > 0)
            {
                template = Request.Form.Files.GetFile("Template") ?? Request.Form.Files.FirstOrDefault();
            }

            // Try common form keys for datasets json (respect case variations)
            string? datasetsJson = request?.DatasetsJson;
            if (string.IsNullOrWhiteSpace(datasetsJson) && Request?.Form != null)
            {
                if (Request.Form.TryGetValue("DatasetsJson", out var v) && v.Count > 0) datasetsJson = v[0];
                else if (Request.Form.TryGetValue("datasetsJson", out var v2) && v2.Count > 0) datasetsJson = v2[0];
                else if (Request.Form.TryGetValue("datasets", out var v3) && v3.Count > 0) datasetsJson = v3[0];
            }

            var normalizedRequest = new DocumentRequest { Template = template, DatasetsJson = datasetsJson };

            var validationResult = _docService.ValidateDocumentRequest(normalizedRequest);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Document generation validation failed: {error}", validationResult.ErrorMessage);
                return BadRequest(validationResult.ErrorMessage);
            }

            #endregion datasets and file Validate

            // Generate documents
            try
            {
                var zip = await _docService.GenerateZipAsync(normalizedRequest.Template!, validationResult.Datasets!, cancellationToken);

                return File(zip, "application/zip", "GeneratedDocuments.zip");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid document generation request.");

                return BadRequest(ex.Message);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Unsupported document format.");

                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return BadRequest("Document generation was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating documents.");

                try
                {
                    await _wrapper.ExceptionLogs.AddAsync
                        ( new AppExceptionLog
                            {
                                Message = ex.Message,
                                StackTrace = ex.StackTrace,
                                Path = HttpContext.Request.Path,
                                Level = "Error"
                            }
                        );

                    await _wrapper.ExceptionLogs.SaveChangesAsync();
                }
                catch
                {
                    // Do not hide the original exception.
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Success = false,
                        Message = "An error occurred while generating documents."
                    });
            }
        }

        #region
        // POST api/document/generate
        // multipart/form-data with 'template' file and 'datasets' JSON array field
        //[HttpPost("generate")]
        //[Consumes("multipart/form-data")]
        //[RequestSizeLimit(50_000_000)]
        //public async Task<IActionResult> Generate([FromForm] DocumentRequest request)
        //{
        //    var template = request?.Template;
        //    var datasetsJson = request?.DatasetsJson;

        //    if (template == null || template.Length == 0) return BadRequest("Template file is required.");

        //    var datasets = JsonSerializer.Deserialize<List<Dictionary<string,string>>>(datasetsJson ?? "[]");
        //    if (datasets == null || datasets.Count == 0) return BadRequest("datasets are required as JSON array of objects.");

        //    try
        //    {
        //        var zip = await _docService.GenerateZipAsync(template, datasets);
        //        return File(zip, "application/zip", "GeneratedDocuments.zip");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating documents");
        //        await _wrapper.ExceptionLogs.AddAsync(new AppExceptionLog { Message = ex.Message, StackTrace = ex.StackTrace, Path = HttpContext.Request.Path, Level = "Error" });
        //        await _wrapper.ExceptionLogs.SaveChangesAsync();
        //        return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while generating documents" });
        //    }
        //}
        #endregion
    }
}
