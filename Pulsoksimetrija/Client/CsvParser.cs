using Client;
using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Client
{
    public class CsvParser
    {
        public List<EcgSample> ParseEcgCsv(string filePath, string participantId)
        {
            List<EcgSample> samples = new List<EcgSample>();

            // Task 4: IDisposable nad StreamReader (using blok)
            using (var reader = new StreamReader(filePath))
            {
                // Task 5: Preskočiti zaglavlje
                reader.ReadLine();

                int rowIndex = 1;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    try
                    {
                        // Task 5: Parsiranje 9 kanala sa InvariantCulture
                        samples.Add(new EcgSample
                        {
                            TimestampMs = long.Parse(parts[0]),
                            EcgMicroV = ParseNullableDouble(parts[1]),
                            HeartRate = ParseNullableDouble(parts[2]),
                            IBI_ms = ParseNullableDouble(parts[3]),
                            AccX = ParseNullableDouble(parts[4]),
                            AccY = ParseNullableDouble(parts[5]),
                            AccZ = ParseNullableDouble(parts[6]),
                            ParticipantId = participantId,
                            RowIndex = rowIndex++
                        });
                    }
                    catch
                    {
                        // Task 5: Problematične redove pišemo u rejects_client.csv
                        File.AppendAllText("rejected_client.csv", $"{line}{Environment.NewLine}");
                    }
                }
            }
            return samples;
        }

        // Task 5: NaN mapirati na null
        private double? ParseNullableDouble(string value)
        {
            if (value.Trim().Equals("NaN", StringComparison.OrdinalIgnoreCase)) return null;
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}