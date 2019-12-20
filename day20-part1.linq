<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

struct State {
	public int x;
	public int y;
	public int moves;

	internal IEnumerable<State> GetNext(Plane<char> plane, Dictionary<(int x, int y), (int x, int y)> warps) {
		if (warps.TryGetValue((x, y), out var t))
			yield return new State { x = t.x, y = t.y, moves = moves + 1 };
		
		if (plane[x - 1, y] == '.')
			yield return new State { x = x - 1, y = y, moves = moves + 1 };
		if (plane[x + 1, y] == '.')
			yield return new State { x = x + 1, y = y, moves = moves + 1 };
		if (plane[x, y - 1] == '.')
			yield return new State { x = x, y = y - 1, moves = moves + 1 };
		if (plane[x, y + 1] == '.')
			yield return new State { x = x, y = y + 1, moves = moves + 1 };
	}
}

void Main() {
	var plane = GetAocCharPlane();

	bool IsLetter2(char c) => c >= 'A' && c <= 'Z';
	bool IsLetter(int x, int y) => IsLetter2(plane[x, y]);

	var spots = new List<(string name, int x, int y)>();

	for (int x = 0; x < plane.Width; x++) {
		for (int y = 0; y < plane.Height; y++) {
			if (plane[x, y] == '.') {
				if (IsLetter(x - 1, y) && IsLetter(x - 2, y)) {
					spots.Add(("" + plane[x - 2, y] + plane[x - 1, y], x, y));
				}
				else if (IsLetter(x, y - 1) && IsLetter(x, y - 2)) {
					spots.Add(("" + plane[x, y - 2] + plane[x, y - 1], x, y));
				}
				else if (IsLetter(x + 1, y) && IsLetter(x + 2, y)) {
					spots.Add(("" + plane[x + 1, y] + plane[x + 2, y], x, y));
				}
				else if (IsLetter(x, y + 1) && IsLetter(x, y + 2)) {
					spots.Add(("" + plane[x, y + 1] + plane[x, y + 2], x, y));
				}
			}
		}
	}

	var pointLookup = spots.ToDictionary(s => (s.x, s.y), s => s.name);
	var nameLookup = spots.ToLookup(s => s.name, s => (s.x, s.y));

	var warps = new Dictionary<(int x, int y), (int x, int y)>();
	foreach (var g in nameLookup) {
		if (g.Count() != 2) continue;
		warps[g.First()] = g.Last();
		warps[g.Last()] = g.First();
	}

	var aa = nameLookup["AA"].Single();
	var zz = nameLookup["ZZ"].Single();
	
	var frontier = new Queue<State>();
	frontier.Enqueue(new State {
		x = aa.x, 
		y = aa.y,
		moves = 0,
	});
	var seen = new HashSet<(int x, int y)>();
	
	while (frontier.Any()) {
		var curr = frontier.Dequeue();
		if (seen.Contains((curr.x, curr.y))) continue;
		seen.Add((curr.x, curr.y));
		
		if ((curr.x, curr.y) == (zz.x, zz.y)) {
			curr.Dump();
			break;
		}
		
		foreach (var next in curr.GetNext(plane, warps)) {
			frontier.Enqueue(next);
		}
	}
}

