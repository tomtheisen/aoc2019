<Query Kind="Program">
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

void Main() {
	var p = new Plane<int>(0, "ABCDEFGHIJ".ToDictionary(k => k - 'A'));
	p[3, 3] = 9;
	p[4, 7] = 4;
	p.Dump("Plane");
}

static string[] GetAocLines() => File.ReadAllLines(Util.CurrentQueryPath.Replace(".linq", ".txt"));
static string GetAocInput() => File.ReadAllText(Util.CurrentQueryPath.Replace(".linq", ".txt"));

static List<int> GetAocIntegers() => GetAocInput().RegexFindAll(@"-?\d+").Select(int.Parse).ToList();
static List<BigInteger> GetAocBigIntegers() => GetAocInput().RegexFindAll(@"-?\d+").Select(BigInteger.Parse).ToList();
static List<uint> GetAocUIntegers() => GetAocInput().RegexFindAll(@"\d+").Select(uint.Parse).ToList();
static List<long> GetAocLongs() => GetAocInput().RegexFindAll(@"-?\d+").Select(long.Parse).ToList();
static List<double> GetAocFloats() => GetAocInput().RegexFindAll(@"-?\d+(\.\d+)?").Select(double.Parse).ToList();

static Plane<char> GetAocCharPlane() {
    var result = new Plane<char>(' ');
    int y = 0;
    foreach (var row in GetAocLines()) {
        for (int x = 0; x < row.Length; x++) result[x, y] = row[x];
        y += 1;
    }
    return result;
}

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
	
	public static Direction TurnLeft(this Direction d) => d switch {
		Direction.Up => Direction.Left,
		Direction.Left => Direction.Down,
		Direction.Down => Direction.Right,
		Direction.Right => Direction.Up,
		_ => throw new ArgumentOutOfRangeException(nameof(d)),
	};

	public static Direction TurnRight(this Direction d) => d switch {
		Direction.Up => Direction.Right,
		Direction.Right => Direction.Down,
		Direction.Down => Direction.Left,
		Direction.Left => Direction.Up,
		_ => throw new ArgumentOutOfRangeException(nameof(d)),
	};
}

public enum Direction { Up, Down, Left, Right }

public struct Point {
	public int X;
	public int Y;
	public Point(int x, int y) => (X, Y) = (x, y);
	public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);

	public Point Up => new Point(X, Y - 1);
	public Point Down => new Point(X, Y + 1);
	public Point Left => new Point(X - 1, Y);
	public Point Right => new Point(X + 1, Y);
	
	public Point[] Neighbors => new[] {Up, Down, Left, Right};
	public Point[] DiagonalNeighbors 
		=> new[] {Up.Left, Up, Up.Right, Left, Right, Down.Left, Down, Down.Right};
		
	public Point Neighbor(Direction d) => d switch {
		Direction.Up => Up, 
		Direction.Down => Down, 
		Direction.Left => Left, 
		Direction.Right => Right,
		_ => throw new ArgumentOutOfRangeException(nameof(d)),
	};
	
	public Point Move(Direction dir, int distance) => dir switch {
		Direction.Up => new Point(X, Y - distance),
		Direction.Down => new Point(X, Y + distance),
		Direction.Left => new Point(X - distance, Y),
		Direction.Right => new Point(X + distance, Y),
	};
}

public struct Bearing {
	public Point Position;
	public Direction Direction;
	
	public Bearing(Point position, Direction direction) {
		Position = position;
		Direction = direction;
	}

	public Bearing Forward => new Bearing(Position.Neighbor(Direction), Direction);
	public Bearing TurnLeft => new Bearing(Position, Direction.TurnLeft());
	public Bearing TurnRight => new Bearing(Position, Direction.TurnLeft());
}

public class Plane<T> where T : notnull {
	public int MinX { get; private set; }
	public int MaxX { get; private set; }
	public int MinY { get; private set; }
	public int MaxY { get; private set; }
	
	public int Width => MaxX - MinX + 1;
	public int Height => MaxY - MinY + 1;
	
	private Dictionary<Point, T> Contents = new Dictionary<Point, T>();
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
		get => Contents.TryGetValue(new Point(x, y), out T t) ? t : Default;
		set {
			if (Contents.Count == 0) {
				MinX = MaxX = x;
				MinY = MaxY = y;
			}
			else {
				MinX = Min(MinX, x);
				MaxX = Max(MaxX, x);
				MinY = Min(MinY, y);
				MaxY = Max(MaxY, y);
			}
			Contents[new Point(x, y)] = value;
		}
	}
	public T this[Point p] {
		get => this[p.X, p.Y];
		set => this[p.X, p.Y] = value;
	}
	
	public int Count => Contents.Count;
	
	public void Clear() {
		Contents.Clear();
		MinX = MaxX = MinY = MaxY = 0;
	}
	
	string ToDump() {
		var sb = new StringBuilder();
		for (int y = MinY; y <= MaxY; y++) {
			for (int x = MinX; x <= MaxX; x++) {
				if (CharMap is {}) sb.Append(CharMap[this[x, y]]);
				else sb.Append(this[x, y]);
			}
			sb.AppendLine();
		}
		return sb.ToString();
	}
}

internal sealed class KeyComparer<T> : IComparer<T> {
    private Func<T, IComparable> KeyGetter;
    public KeyComparer(Func<T, IComparable> keyGetter) => KeyGetter = keyGetter;
    public int Compare(T x, T y) => KeyGetter(x).CompareTo(KeyGetter(y));
}

static class PriorityQueue {
    public static PriorityQueue<T> Create<T>() where T : IComparable
        => new PriorityQueue<T>(n => n);
}

class PriorityQueue<T> : IReadOnlyCollection<T> {
    private List<T> Heap = new List<T>();
    private IComparer<T> Comparer;
    
    public PriorityQueue(IComparer<T> comparer) {
        Comparer = comparer;
    }
    public PriorityQueue(Func<T, IComparable> priorityGetter)
        : this(new KeyComparer<T>(priorityGetter)) { }

    [Obsolete]
    private void AssertHeapification() {
        for (int i = 1; i < Heap.Count; i++)
            if (Comparer.Compare(Heap[i], Heap[i - 1 >> 1]) > 0)
                throw new Exception($"bad heapify at {i}");
    }
    
    private void Reheap(int i) {
        for (int j; i > 0; i = j) {
            j = i - 1 >> 1;
            if (Comparer.Compare(Heap[i], Heap[j]) <= 0) break;
            (Heap[i], Heap[j]) = (Heap[j], Heap[i]);
        }
        //AssertHeapification();
    }

    public void Add(T item) {
        Heap.Add(item ?? throw new ArgumentNullException(nameof(item)));
        Reheap(Heap.Count - 1);
    }
    public T Peek() => Heap.Count > 0 ? Heap[0]
        : throw new InvalidOperationException("Queue Empty");
    public T Dequeue() {
        var result = Peek();
        int i = 0;
        for (; i * 2 + 1 < Heap.Count; ) {
            int a = 2 * i + 1, b = a + 1;
            bool useA = b == Heap.Count || Comparer.Compare(Heap[a], Heap[b]) > 0;
            Heap[i] = Heap[i = useA ? a: b];
        }
        Heap[i] = Heap[^1];
        Reheap(i);
        Heap.RemoveAt(Heap.Count - 1);
        return result;
    }

    public int Count => Heap.Count;
    public bool IsReadOnly => false;
    public void Clear() => Heap.Clear();
    public bool Contains(T item) => Heap.Contains(item);
    public IEnumerator<T> GetEnumerator() => Heap.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}