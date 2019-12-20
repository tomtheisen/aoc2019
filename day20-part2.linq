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
	public int depth;
	public int moves;

	internal IEnumerable<State> GetNext(
		Plane<char> plane, 
		Dictionary<(int x, int y), (int x, int y)> warps) 
	{
		bool IsOuter(int x, int y)
			=> x == 2 || y == 2 || x == plane.Width - 3 || y == plane.Height - 3;

		if (warps.TryGetValue((x, y), out var t)) {
			if (!IsOuter(x, y)) yield return new State { 
				x = t.x, y = t.y, depth = depth + 1, moves = moves + 1 };
			else if (depth > 0) yield return new State { 
				x = t.x, y = t.y, depth = depth - 1, moves = moves + 1 };
		}
		
		if (plane[x - 1, y] == '.')
			yield return new State { x = x - 1, y = y, depth = depth, moves = moves + 1, /* path = path + (x-1, y) + " " */ };
		if (plane[x + 1, y] == '.')
			yield return new State { x = x + 1, y = y, depth = depth, moves = moves + 1, /* path = path + (x+1, y) + " " */ };
		if (plane[x, y - 1] == '.')
			yield return new State { x = x, y = y - 1, depth = depth, moves = moves + 1, /* path = path + (x, y-1) + " " */ };
		if (plane[x, y + 1] == '.')
			yield return new State { x = x, y = y + 1, depth = depth, moves = moves + 1, /* path = path + (x, y+1) + " " */ };
	}
}

void Main() {
	var plane = GetAocCharPlane();
	bool IsLetter(int x, int y) => plane[x, y] >= 'A' && plane[x, y] <= 'Z';

	var spots = new List<(string name, int x, int y)>();
	for (int x = 0; x < plane.Width; x++) {
		for (int y = 0; y < plane.Height; y++) {
			if (plane[x, y] == '.') {
				if (IsLetter(x - 1, y) && IsLetter(x - 2, y))
					spots.Add(("" + plane[x - 2, y] + plane[x - 1, y], x, y));
				else if (IsLetter(x, y - 1) && IsLetter(x, y - 2))
					spots.Add(("" + plane[x, y - 2] + plane[x, y - 1], x, y));
				else if (IsLetter(x + 1, y) && IsLetter(x + 2, y))
					spots.Add(("" + plane[x + 1, y] + plane[x + 2, y], x, y));
				else if (IsLetter(x, y + 1) && IsLetter(x, y + 2))
					spots.Add(("" + plane[x, y + 1] + plane[x, y + 2], x, y));
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

	(int x, int y) aa = nameLookup["AA"].Single(), zz = nameLookup["ZZ"].Single();

	var frontier = new Queue<State>();
	frontier.Enqueue(new State { x = aa.x,  y = aa.y });
	var seen = new HashSet<(int x, int y, int depth)>();
	
	var visitedContainer = new DumpContainer().Dump("Visited Count");
	var maxDepthContainer = new DumpContainer().Dump("Max Depth");
	
	int visited = 0, maxdepth = 0;
	
	while (frontier.Any()) {
		var curr = frontier.Dequeue();
		if (seen.Contains((curr.x, curr.y, curr.depth))) continue;
		seen.Add((curr.x, curr.y, curr.depth));
		
		visitedContainer.Content = ++visited;
		maxDepthContainer.Content = maxdepth = Max(maxdepth, curr.depth);
		
		if ((curr.x, curr.y, curr.depth) == (zz.x, zz.y, 0)) {
			curr.moves.Dump();
			break;
		}
		
		foreach (var next in curr.GetNext(plane, warps)) frontier.Enqueue(next);
	}
}
