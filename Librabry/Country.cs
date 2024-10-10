namespace Librabry
{
    public class Country
    {
        public CountryName Name { get; set; }
        public List<string> Capital { get; set; } = new List<string>();
        public string Region { get; set; } = "N/A";
        public string Subregion { get; set; } = "N/A";
        public int Population { get; set; }
        public Dictionary<string, double> Gini { get; set; } = new Dictionary<string, double>();
        public Flags Flags { get; set; }
        public Dictionary<string, string> Languages { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, Currency> Currencies { get; set; } = new Dictionary<string, Currency>();
        public double Area { get; set; }
        public bool? Independent { get; set; }
        public string Status { get; set; } = "N/A";
        public bool? UnMember { get; set; }

        public string OutputCurrencies => Currencies.Any() ? string.Join("\n", Currencies.Values.Select(c => $"{c.name} - {c.symbol}")) + "\n" : "N/A\n";
        public string OutputLanguages => Languages.Any() ? string.Join("\n", Languages.Values) + "\n" : "N/A\n";
        public string OutPutUnMember => UnMember.HasValue ? (UnMember.Value ? "Yes" : "No") : "N/A";
        public string OutputIndependent => Independent.HasValue ? (Independent.Value ? "Yes" : "No") : "N/A";
        public string OutPutArea => Area != 0 ? Area.ToString() : "N/A";
        public string OutputGini => Gini.Any() ? string.Join("\n", Gini.Values) + "\n" : "N/A\n";
        public string OutputPopulation => Population != 0 ? Population.ToString() : "N/A";
        public string OutputCapital => Capital.Any() ? string.Join("\n", Capital) + "\n" : "N/A\n";
    }
}
