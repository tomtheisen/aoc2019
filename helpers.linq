<Query Kind="Program">
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

void Main() {
	var p = new Plane<int>(0, "ABCDEFGHIJ".ToDictionary(k => k - 'A'));
	p[3, 3] = 9;
	p[4, 7] = 4;
	p.Dump("Plane");
	p.YGoesUp = true;
	p.Dump("Plane");
}

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
	
	public static string? RegexFind(this string @this, string pattern)
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

public class Plane<T> where T : notnull {
	struct Position {
		public int X;
		public int Y;
		public Position(int x, int y) => (X, Y) = (x, y);
	}

	public bool YGoesUp { get; set; } = false;

	public int MinX { get; private set; }
	public int MaxX { get; private set; }
	public int MinY { get; private set; }
	public int MaxY { get; private set; }
	
	private Dictionary<Position, T> Contents = new Dictionary<Position, T>();
	private IReadOnlyDictionary<T, char>? CharMap = null;
	private T Default;
	public Plane(T @default, IReadOnlyDictionary<T, char>? charmap = null) {
		Default = @default;
		CharMap = charmap;
	}
	public Plane(T @default, params (T, char)[] charmap)
		: this(@default, charmap.ToDictionary(t => t.Item1, t => t.Item2))
	{ }

	public T this[int x, int y] {
		get => Contents.TryGetValue(new Position(x, y), out T t) ? t : Default;
		set {
			if (Contents.Count == 0) {
				MinX = MaxX = x;
				MinY = MaxY = y;
			}
			else {
				MinX = Min(MinX, x);
				MaxX = Max(MaxX, x);
				MinY = Min(MinY, y);
				MaxY = Max(MaxX, y);
			}
			Contents[new Position(x, y)] = value;
		}
	}
	
	public int Count => Contents.Count;
	
	public void Clear() {
		Contents.Clear();
		MinX = MaxX = MinY = MaxY = 0;
	}
	
	string ToDump() {
		var sb = new StringBuilder();
		for (int y = YGoesUp ? MaxY : MinY; YGoesUp ? y >= MinY : y <= MaxY; y += YGoesUp ? -1 : 1) {
			for (int x = MinX; x <= MaxX; x++) {
				if (CharMap is {}) sb.Append(CharMap[this[x, y]]);
				else sb.Append(this[x, y]);
			}
			sb.AppendLine();
		}
		return sb.ToString();
	}
}
