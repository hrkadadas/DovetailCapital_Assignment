using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FileGenerationAPIApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;

namespace FileGenerationAPIApp.Services
{
    public class DocumentGeneratorService : IDocumentGeneratorService
    {
        private readonly FileGenerationAPIApp.Wrappers.IWrapper _wrapper;

        public DocumentGeneratorService(FileGenerationAPIApp.Wrappers.IWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public DocumentRequestValidationResult ValidateDocumentRequest(Models.DocumentRequest? request)
        {
            if (request == null) return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "Request is required." };
            if (request.Template == null || request.Template.Length == 0) return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "Template is required." };
            if (string.IsNullOrWhiteSpace(request.DatasetsJson)) return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "DatasetsJson is required." };
            try
            {
                var json = request.DatasetsJson ?? string.Empty;
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                List<Dictionary<string, string>> datasets;

                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    datasets = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json, options) ?? new List<Dictionary<string, string>>();
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    var single = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, options) ?? new Dictionary<string, string>();
                    datasets = new List<Dictionary<string, string>> { single };
                }
                else
                {
                    return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "DatasetsJson must be a JSON object or array." };
                }

                if (datasets == null || datasets.Count == 0) return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "At least one dataset is required." };
                return new DocumentRequestValidationResult { IsValid = true, Datasets = datasets };
            }
            catch (System.Text.Json.JsonException)
            {
                return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "DatasetsJson must be valid JSON." };
            }
            catch
            {
                return new DocumentRequestValidationResult { IsValid = false, ErrorMessage = "DatasetsJson must be valid JSON." };
            }
        }

        public async Task<byte[]> GenerateZipAsync(IFormFile template, List<Dictionary<string, string>> datasets, CancellationToken cancellationToken = default)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (datasets == null || datasets.Count == 0) throw new ArgumentException("datasets required", nameof(datasets));

            using var templateStream = new MemoryStream();
            await template.CopyToAsync(templateStream, cancellationToken);

            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // load masters from repositories 
                var roles = await _wrapper.Roles.GetAllAsync();
                var designations = await _wrapper.Designations.GetAllAsync();
                var departments = await _wrapper.Departments.GetAllAsync();

                var roleMap = roles.ToDictionary(r => r.Id, r => r.Name);
                var desMap = designations.ToDictionary(d => d.Id, d => d.Name);
                var depMap = departments.ToDictionary(d => d.Id, d => d.Name);

                // transform incoming datasets to only expose required user fields (firstname, lastname, username)
                // and resolve role/designation/department names using the database relationships 
                var transformed = new List<Dictionary<string, string>>();

                // load users once so we can resolve related ids by username
                var users = await _wrapper.Users.GetAllAsync();

                foreach (var raw in datasets)
                {
                    var t = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    string getRaw(params string[] keys)
                    {
                        foreach (var k in keys)
                        {
                            if (raw.TryGetValue(k, out var v) && v != null) return v;
                        }
                        return string.Empty;
                    }

                    var firstname = getRaw("firstname", "FirstName", "Firstname") ?? string.Empty;
                    var lastname = getRaw("lastname", "LastName", "Lastname") ?? string.Empty;
                    var username = getRaw("username", "Username", "UserName", "userName") ?? string.Empty;

                    // Use PascalCase keys so templates using {{FirstName}} etc. are matched
                    t["FirstName"] = firstname;
                    t["LastName"] = lastname;
                    t["UserName"] = username;

                    // resolve related names using stored user record if available (match by username)
                    string roleName = string.Empty;
                    string designationName = string.Empty;
                    string departmentName = string.Empty;

                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                        if (user != null)
                        {
                            // If dataset is missing first/last name, use values from the user record
                            if (string.IsNullOrWhiteSpace(firstname)) firstname = user.Firstname ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(lastname)) lastname = user.Lastname ?? string.Empty;
                            if (user.Role != null) roleName = user.Role.Name;
                            else if (roleMap.TryGetValue(user.RoleId, out var rn)) roleName = rn;

                            if (user.Designation != null) designationName = user.Designation.Name;
                            else if (user.DesignationId.HasValue && desMap.TryGetValue(user.DesignationId.Value, out var dn)) designationName = dn;

                            if (user.Department != null) departmentName = user.Department.Name;
                            else if (user.DepartmentId.HasValue && depMap.TryGetValue(user.DepartmentId.Value, out var dpn)) departmentName = dpn;
                        }
                    }

                    t["RoleName"] = roleName;
                    t["DesignationName"] = designationName;
                    t["DepartmentName"] = departmentName;

                    transformed.Add(t);
                }

                int idx = 1;
                foreach (var data in transformed)
                {
                    templateStream.Position = 0;
                    using var mem = new MemoryStream();
                    await templateStream.CopyToAsync(mem, cancellationToken);
                    mem.Position = 0;

                    using (var word = WordprocessingDocument.Open(mem, true))
                    {
                        void ProcessTextNodes(List<Text> texts)
                        {
                            if (texts == null || texts.Count == 0) return;

                            foreach (var kv in data)
                            {
                                var token = "{{" + kv.Key + "}}";
                                while (true)
                                {
                                    var combined = string.Concat(texts.Select(t => t.Text ?? string.Empty));
                                    // find token case-insensitively so templates using {{firstname}} or {{FirstName}} both match
                                    var idxToken = combined.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                                    if (idxToken < 0) break;

                                    int cur = 0;
                                    int startNode = -1, endNode = -1;
                                    int startOffset = 0, endOffset = 0;
                                    for (int i = 0; i < texts.Count; i++)
                                    {
                                        var len = (texts[i].Text ?? string.Empty).Length;
                                        if (startNode == -1 && idxToken >= cur && idxToken < cur + len)
                                        {
                                            startNode = i; startOffset = idxToken - cur;
                                        }
                                        if (endNode == -1 && (idxToken + token.Length) > cur && (idxToken + token.Length) <= cur + len)
                                        {
                                            endNode = i; endOffset = (idxToken + token.Length) - cur; break;
                                        }
                                        cur += len;
                                    }
                                    if (startNode == -1 || endNode == -1) break;

                                    var startText = texts[startNode].Text ?? string.Empty;
                                    var endText = texts[endNode].Text ?? string.Empty;

                                    var prefix = startText.Substring(0, startOffset);
                                    var suffix = endText.Substring(endOffset);

                                    if (startNode == endNode)
                                    {
                                        // token is fully inside a single Text node
                                        texts[startNode].Text = prefix + (kv.Value ?? string.Empty) + suffix;
                                    }
                                    else
                                    {
                                        texts[startNode].Text = prefix + (kv.Value ?? string.Empty);
                                        for (int j = startNode + 1; j < endNode; j++) texts[j].Text = string.Empty;
                                        texts[endNode].Text = suffix;
                                    }
                                }
                            }
                        }

                        if (word.MainDocumentPart?.Document != null)
                        {
                            var mainTexts = word.MainDocumentPart.Document.Descendants<Text>().ToList();
                            ProcessTextNodes(mainTexts);
                        }

                        if (word.MainDocumentPart?.HeaderParts != null)
                        {
                            foreach (var header in word.MainDocumentPart.HeaderParts)
                            {
                                var headerTexts = header.RootElement.Descendants<Text>().ToList();
                                ProcessTextNodes(headerTexts);
                                header.Header.Save();
                            }
                        }

                        if (word.MainDocumentPart?.FooterParts != null)
                        {
                            foreach (var footer in word.MainDocumentPart.FooterParts)
                            {
                                var footerTexts = footer.RootElement.Descendants<Text>().ToList();
                                ProcessTextNodes(footerTexts);
                                footer.Footer.Save();
                            }
                        }
                        word.MainDocumentPart?.Document.Save();
                    }

                    mem.Position = 0;
                    //var entry = archive.CreateEntry($"Document_{idx}.docx", CompressionLevel.Optimal);
                    var entry = archive.CreateEntry($"{data["firstname"]}_{data["lastname"]}_Document.docx", CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    mem.CopyTo(entryStream);
                    idx++;
                }
            }

            zipStream.Position = 0;
            return zipStream.ToArray();
        }
    }
}
