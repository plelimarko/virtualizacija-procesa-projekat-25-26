using Client;
using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Client
{
    public class CsvParser
    {
        public List<EcgSample> ParseEcgCsv(string filePath, string participantId)
        {
            List<EcgSample> samples = new List<EcgSample>();
            string rejectPath = Path.Combine(
                Path.GetDirectoryName(filePath), "rejected_client.csv");

            using (StreamReader reader = new StreamReader(filePath))
            using (StreamWriter rejectWriter = new StreamWriter(rejectPath, false))
            {
                reader.ReadLine();

                int rowIndex = 1;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    try
                    {
                        if (parts.Length < 3)
                        {
                            throw new FormatException("Red nema dovoljno kolona.");
                        }

                        samples.Add(new EcgSample
                        {
                            TimestampMs = long.Parse(parts[0], CultureInfo.InvariantCulture),
                            EcgMicroV = ParseNullableDouble(parts[2]),
                            HeartRate = null,
                            IBI_ms = null,
                            AccX = null,
                            AccY = null,
                            AccZ = null,
                            ParticipantId = participantId,
                            RowIndex = rowIndex++
                        });
                    }
                    catch (Exception ex)
                    {
                        rejectWriter.WriteLine($"{rowIndex},{ex.Message},{line}");
                        rowIndex++;
                    }
                }
            }
            return samples;
        }

        private double? ParseNullableDouble(string value)
        {
            if (value.Trim().Equals("NaN", StringComparison.OrdinalIgnoreCase)) return null;
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}