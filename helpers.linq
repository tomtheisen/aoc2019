<Query Kind="Program" />

void Main() {}

string[] GetAocLines() => File.ReadAllLines(Util.CurrentQueryPath.Replace(".linq", ".txt"));
string GetAocInput() => File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", ".txt"));

List<int> GetAocIntegers() => GetAocInput().RegexFindAll(@"-?\d+").Select(int.Parse).ToList();
List<uint> GetAocUIntegers() => GetAocInput().RegexFindAll(@"\d+").Select(uint.Parse).ToList();
List<long> GetAocLongs() => GetAocInput().RegexFindAll(@"-?\d+").Select(long.Parse).ToList();
List<double> GetAocFloats() => GetAocInput().RegexFindAll(@"-?\d+(\.\d+)?").Select(double.Parse).ToList();

public static partial class Extensions {
	public static List<string> RegexFindAll(this string @this, string pattern)
		=> Regex.Matches(@this, pattern).Where(m => m.Success).Select(m => m.Value).ToList();
	
	public static string RegexFind(this string @this, string pattern)
		=> Regex.Matches(@this, pattern).FirstOrDefault(m => m.Success)?.Value;
}
