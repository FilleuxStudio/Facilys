using System.Runtime;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Facilys.Components.Services
{
    public class VINDecoderService
    {
        private static readonly Dictionary<string, string> manufacturers = new Dictionary<string, string>
{
    {"WMI", "Constructeur"},
    {"WBA", "BMW"},
    {"WBS", "BMW M GmbH"},
    {"WDB", "Mercedes-Benz"},
    {"WDD", "Mercedes-Benz"},
    {"WF0", "Ford (Allemagne)"},
    {"1FA", "Ford (USA)"},
    {"VF1", "Renault"},
    {"VF3", "Peugeot"},
    {"VF7", "Citroën"},
    {"WAU", "Audi"},
    {"WUA", "Audi (Gamme RS)"},
    {"WP0", "Porsche"},
    {"WP1", "Porsche SUV"},
    {"W0L", "Opel"},
    {"WVW", "Volkswagen"},
    {"ZAR", "Alfa Romeo"},
    {"ZFF", "Ferrari"},
    {"ZFA", "Fiat"},
    {"1HD", "Harley-Davidson"},
    {"SAJ", "Jaguar"},
    {"1J4", "Jeep"},
    {"KNA", "Kia"},
    {"SAL", "Land Rover"},
    {"JM", "Mazda"},
    {"VSS", "Seat"},
    {"TMB", "Škoda Auto"},
    {"JT", "Toyota"},
    {"VF6", "Renault Trucks"},
    {"VF8", "Matra"},
    {"VF9", "Bugatti"},
    {"VFA", "Alpine Renault"},
    {"VNV", "Nissan (Utilitaires)"},
    {"VR1", "DS Automobiles"},
    {"JN", "Nissan"},
    {"JH", "Honda"},
    {"JF", "Subaru"},
    {"MA3", "Suzuki Inde"},
    {"UU1", "Dacia"}
};

        private static readonly Dictionary<char, string> countries = new Dictionary<char, string>
{
    {'1', "États-Unis"},
    {'2', "Canada"},
    {'3', "Mexique"},
    {'J', "Japon"},
    {'K', "Corée"},
    {'L', "Chine"},
    {'S', "Royaume-Uni"},
    {'V', "France"},
    {'W', "Allemagne"},
    {'Z', "Italie"},
    {'Y', "Suède"},
    {'T', "Suisse"},
    {'6', "Australie"},
    {'9', "Brésil"},
    {'8', "Argentine"},
    {'M', "Inde"},
    {'N', "Turquie"},
    {'X', "Russie"},
    {'4', "États-Unis"},
    {'5', "États-Unis"}
        };

        public VINInfo DecodeVIN(string vin)
        {
            if (vin.Length != 17)
            {
                return new VINInfo();
            }

            VINInfo info = new()
            {
                VIN = vin,
                Manufacturer = GetManufacturer(vin),
                Country = GetCountry(vin),
                Year = GetYear(vin),
                AssemblyPlant = vin[10],
                SerialNumber = vin.Substring(11),
                ValidVIN = ValidateVIN(vin),
            };

            return info; 
        }

        private static string GetCountry(string vin)
        {
            return countries.TryGetValue(vin[0], out string country) ? country : "Inconnu";
        }

        private static string GetManufacturer(string vin)
        {
            string wmi = vin.Substring(0, 3);
            return manufacturers.TryGetValue(wmi, out string manufacturer) ? manufacturer : "Inconnu";
        }

        private static int GetYear(string vin)
        {
            char yearCode = vin[9];
            string yearCodes = "ABCDEFGHJKLMNPRSTVWXY123456789";
            int yearIndex = yearCodes.IndexOf(yearCode);
            return (yearIndex >= 0) ? yearIndex + 2010 : 0;
        }

        private static bool ValidateVIN(string vin)
        {
            string validChars = "0123456789ABCDEFGHJKLMNPRSTUVWXYZ";
            int[] weights = { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                char c = vin[i];
                if (!validChars.Contains(c)) return false;
                int value = validChars.IndexOf(c);
                if (i == 8) continue; // Skip check digit
                sum += value * weights[i];
            }

            int checkDigit = sum % 11;
            return (checkDigit == 10 ? 'X' : checkDigit.ToString()[0]) == vin[8];
        }
    }

    public class VINInfo
    {
        public string VIN { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
            public char AssemblyPlant { get; set; } = ' ';
            public string SerialNumber { get; set; } = string.Empty;
            public bool ValidVIN { get; set; } = false;
    }
}
