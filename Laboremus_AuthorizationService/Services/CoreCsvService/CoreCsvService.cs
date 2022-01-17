using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.TypeConversion;
using Laboremus_AuthorizationService.DTOs;
using Microsoft.Extensions.Options;

namespace Laboremus_AuthorizationService.Services.CoreCsvService
{
    public class CoreCsvService : ICoreCsvService
    {
        private readonly IFileSystem _fileSystem;
        private readonly ExportSettings _exportSettings;

        public CoreCsvService(IOptions<ExportSettings> exportOptions) : this(new FileSystem(), exportOptions)
        {
            _exportSettings = exportOptions.Value;
        }

        public CoreCsvService(IFileSystem fileSystem, IOptions<ExportSettings> exportOptions)
        {
            _exportSettings = exportOptions.Value;
            _fileSystem = fileSystem;
        }

        public async Task WriteRecordsToCsvFileAsync<T>(string fullPath, int page, List<T> records)
        {
            if (page == 1)
            {
                using (var writer = new StreamWriter(fullPath))
                {
                    await WriteRecordsAsync(records, writer);
                    return;
                }
            }

            using (var stream = _fileSystem.File.Open(fullPath, FileMode.Append))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await WriteRecordsAsync(records, writer, false);
                }
            }
        }

        private async Task WriteRecordsAsync<T>(List<T> records, StreamWriter writer, bool hasHeader = true)
        {
            var options = new TypeConverterOptions
            {
                Formats = new[] { "dd.MM.yyyy hh:mm:ss" }
            };

            using (var csv = new CsvWriter(writer, new CultureInfo(_exportSettings.Culture)))
            {
                csv.Configuration.HasHeaderRecord = hasHeader;
                csv.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                await csv.WriteRecordsAsync(records);
            }
        }
    }
}
