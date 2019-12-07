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

	public static IEnumerable<List<T>> Permutations<T>(this IReadOnlyList<T> ts) {
		var chosen = new bool[ts.Count];

		for (int i = 0, remain = i; ; remain = ++i) {
			Array.Fill(chosen, false);
			var current = new List<T>();

			while (current.Count < ts.Count) {
				int pending = ts.Count - current.Count;
				int count = remain % pending, idx = 0;

				// appears to be significantly faster than Array.Find for some reason
				while (chosen[idx]) idx++;
				for (int j = 0; j < count; j++) do idx++; while (chosen[idx]);

				chosen[idx] = true;
				current.Add(ts[idx]);
				remain /= pending;
			}
			if (remain > 0) yield break;
			yield return current;
		}
	}
}
