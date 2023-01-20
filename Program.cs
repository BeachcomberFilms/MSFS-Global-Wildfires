using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;

class DownloadWildfireData
{
    static async Task Main(string[] args)
    {
        using (var client = new HttpClient())
        {
            using (var response = await client.GetAsync("https://firms.modaps.eosdis.nasa.gov/data/active_fire/suomi-npp-viirs-c2/csv/SUOMI_VIIRS_C2_Global_24h.csv"))
            {
                response.EnsureSuccessStatusCode();
                using (var fileStream = await response.Content.ReadAsStreamAsync())
                {
                    // Make sure the directory exists
                    Directory.CreateDirectory("c:\\fires");
                    using (var output = File.Create("c:\\fires\\SUOMI_VIIRS_C2_Global_24h.csv"))
                    {
                        await fileStream.CopyToAsync(output);
                    }
                }
            }
        }

        // Load the CSV file
        var csvData = File.ReadAllLines("c:\\fires\\SUOMI_VIIRS_C2_Global_24h.csv").Skip(1).Select(line => line.Split(','));

        // Create the XML document
        var xmlDoc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("FSData", new XAttribute("version", "9.0"),
                from data in csvData
                select new XElement("SceneryObject",
                    new XAttribute("groupIndex", "5"),
                    new XAttribute("lat", data[0]),
                    new XAttribute("lon", data[1]),
                    new XAttribute("alt", "0"),
                    new XAttribute("pitch", "0"),
                    new XAttribute("bank", "0"),
                    new XAttribute("heading", "-179.999995"),
                    new XAttribute("imageComplexity", "VERY_SPARSE"),
                    new XAttribute("altitudeIsAgl", "TRUE"),
                    new XAttribute("snapToGround", "TRUE"),
                    new XAttribute("snapToNormal", "FALSE")
                )
            )
        );

        // Save the XML file
        xmlDoc.Save("c:\\fires\\objects.xml");
    }
}
