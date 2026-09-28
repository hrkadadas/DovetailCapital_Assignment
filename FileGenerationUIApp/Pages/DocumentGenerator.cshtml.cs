using System.IO.Compression;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace FileGenerationUIApp.Pages
{
    public class DocumentGeneratorModel : PageModel
    {
        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync(IFormFile? Template, string? DatasetsJson)
        {
            if (Template == null || string.IsNullOrWhiteSpace(DatasetsJson))
            {
                ModelState.AddModelError(string.Empty, "Template and datasets are required.");
                return Page();
            }

            // read the uploaded template into memory
            byte[] templateBytes;
            using (var ms = new MemoryStream())
            {
                await Template.CopyToAsync(ms);
                templateBytes = ms.ToArray();
            }

            // parse datasets JSON (expected: array of objects)
            List<Dictionary<string, object>>? datasets;
            try
            {
                datasets = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(DatasetsJson!, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                ModelState.AddModelError(string.Empty, "Invalid datasets JSON: " + ex.Message);
                return Page();
            }

            if (datasets == null || datasets.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No datasets provided.");
                return Page();
            }

            // helper to build dictionary of replacement tokens for a dataset
            Dictionary<string, string> BuildTokenMap(Dictionary<string, object> src)
            {
                var map = new Dictionary<string, string>();
                foreach (var kv in src)
                {
                    var key = kv.Key ?? string.Empty;
                    var val = kv.Value?.ToString() ?? string.Empty;
                    // add multiple key variants so tokens in templates like {{firstname}} or {{FirstName}} both work
                    if (!map.ContainsKey(key)) map[key] = val;
                    var lower = key.ToLowerInvariant();
                    if (!map.ContainsKey(lower)) map[lower] = val;
                    // also add camelCase version if different
                    if (char.IsUpper(key[0]))
                    {
                        var camel = char.ToLowerInvariant(key[0]) + key.Substring(1);
                        if (!map.ContainsKey(camel)) map[camel] = val;
                    }
                }
                return map;
            }

            // if only one dataset, return single .docx, otherwise return a zip
            if (datasets.Count == 1)
            {
                var tokenMap = BuildTokenMap(datasets[0]);
                using var outStream = new MemoryStream();
                // copy template to stream
                await outStream.WriteAsync(templateBytes, 0, templateBytes.Length);
                outStream.Position = 0;
                ReplacePlaceholders(outStream, tokenMap);
                outStream.Position = 0;
                var fileName = "generated_document.docx";
                // attempt to produce better filename using firstname/lastname
                var fn = tokenMap.TryGetValue("firstname", out var f) ? f : (tokenMap.TryGetValue("FirstName", out var f2) ? f2 : string.Empty);
                var ln = tokenMap.TryGetValue("lastname", out var l) ? l : (tokenMap.TryGetValue("LastName", out var l2) ? l2 : string.Empty);
                if (!string.IsNullOrWhiteSpace(fn) || !string.IsNullOrWhiteSpace(ln))
                {
                    var safe = System.IO.Path.GetInvalidFileNameChars();
                    var name = (fn + "_" + ln).Trim('_');
                    foreach (var c in safe) name = name.Replace(c, '-');
                    fileName = name + ".docx";
                }

                return File(outStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }

            // multiple datasets -> create zip
            using var zipMs = new MemoryStream();
            using (var archive = new ZipArchive(zipMs, ZipArchiveMode.Create, true))
            {
                var idx = 1;
                foreach (var ds in datasets)
                {
                    var tokenMap = BuildTokenMap(ds);
                    using var docStream = new MemoryStream();
                    await docStream.WriteAsync(templateBytes, 0, templateBytes.Length);
                    docStream.Position = 0;
                    ReplacePlaceholders(docStream, tokenMap);
                    docStream.Position = 0;

                    var entryName = $"document_{idx}.docx";
                    var fn = tokenMap.TryGetValue("firstname", out var f) ? f : (tokenMap.TryGetValue("FirstName", out var f2) ? f2 : string.Empty);
                    var ln = tokenMap.TryGetValue("lastname", out var l) ? l : (tokenMap.TryGetValue("LastName", out var l2) ? l2 : string.Empty);
                    if (!string.IsNullOrWhiteSpace(fn) || !string.IsNullOrWhiteSpace(ln))
                    {
                        var safe = System.IO.Path.GetInvalidFileNameChars();
                        var name = (fn + "_" + ln).Trim('_');
                        foreach (var c in safe) name = name.Replace(c, '-');
                        entryName = name + ".docx";
                    }

                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    await docStream.CopyToAsync(entryStream);
                    idx++;
                }
            }

            zipMs.Position = 0;
            return File(zipMs.ToArray(), "application/zip", "generated_documents.zip");
        }

        private void ReplacePlaceholders(Stream docStream, Dictionary<string, string> data)
        {
            // Opens the document for editing
            using var wordDoc = WordprocessingDocument.Open(docStream, true);
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart != null)
            {
                var texts = mainPart.Document.Descendants<Text>();
                foreach (var text in texts)
                {
                    var value = text.Text;
                    if (string.IsNullOrEmpty(value))
                        continue;

                    foreach (var kv in data)
                    {
                        var token = "{{" + kv.Key + "}}";
                        if (value.Contains(token))
                        {
                            value = value.Replace(token, kv.Value ?? string.Empty);
                        }
                    }

                    text.Text = value;
                }

                mainPart.Document.Save();
            }

            // Optionally replace in headers/footers
            if (wordDoc.MainDocumentPart?.HeaderParts != null)
            {
                foreach (var header in wordDoc.MainDocumentPart.HeaderParts)
                {
                    foreach (var text in header.RootElement.Descendants<Text>())
                    {
                        var value = text.Text;
                        foreach (var kv in data)
                        {
                            var token = "{{" + kv.Key + "}}";
                            if (value.Contains(token)) value = value.Replace(token, kv.Value ?? string.Empty);
                        }
                        text.Text = value;
                    }
                    header.Header.Save();
                }
            }

            if (wordDoc.MainDocumentPart?.FooterParts != null)
            {
                foreach (var footer in wordDoc.MainDocumentPart.FooterParts)
                {
                    foreach (var text in footer.RootElement.Descendants<Text>())
                    {
                        var value = text.Text;
                        foreach (var kv in data)
                        {
                            var token = "{{" + kv.Key + "}}";
                            if (value.Contains(token)) value = value.Replace(token, kv.Value ?? string.Empty);
                        }
                        text.Text = value;
                    }
                    footer.Footer.Save();
                }
            }
        }
    }
}
