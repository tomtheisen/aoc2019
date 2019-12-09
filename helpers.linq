<Query Kind="Program">
  <Namespace>System.Numerics</Namespace>
</Query>

void Main() {}
static string[] GetAocLines() => File.ReadAllLines(Util.CurrentQueryPath.Replace(".linq", ".txt"));
static string GetAocInput() => File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", ".txt"));

static List<int> GetAocIntegers() => GetAocInput().RegexFindAll(@"-?\d+").Select(int.Parse).ToList();
static List<BigInteger> GetAocBigIntegers() => GetAocInput().RegexFindAll(@"-?\d+").Select(BigInteger.Parse).ToList();
static List<uint> GetAocUIntegers() => GetAocInput().RegexFindAll(@"\d+").Select(uint.Parse).ToList();
static List<long> GetAocLongs() => GetAocInput().RegexFindAll(@"-?\d+").Select(long.Parse).ToList();
static List<double> GetAocFloats() => GetAocInput().RegexFindAll(@"-?\d+(\.\d+)?").Select(double.Parse).ToList();

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
