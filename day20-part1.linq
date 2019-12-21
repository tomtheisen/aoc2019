<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

struct State {
	public Point Position;
	public int Moves;
}

void Main() {
	var search = new PlutoMaze(GetAocCharPlane());
	search.Search().Moves.Dump();
}

class PlutoMaze : BreadthFirst<State, Point> {
	private Point Target;
	private Dictionary<Point, Point> Warps = new Dictionary<Point, Point>();
	private Plane<char> Plane;

	private bool IsLetter(int x, int y) => Plane[x, y] is char c && c >= 'A' && c <= 'Z';

	public PlutoMaze(Plane<char> plane) {
		Plane = plane;

		var spots = new List<(string name, Point position)>();
		for (int x = 0; x < plane.Width; x++) {
			for (int y = 0; y < plane.Height; y++) {
				if (plane[x, y] == '.') {
					if (IsLetter(x - 1, y) && IsLetter(x - 2, y))
						spots.Add(("" + plane[x - 2, y] + plane[x - 1, y], new Point(x, y)));
					else if (IsLetter(x, y - 1) && IsLetter(x, y - 2))
						spots.Add(("" + plane[x, y - 2] + plane[x, y - 1], new Point(x, y)));
					else if (IsLetter(x + 1, y) && IsLetter(x + 2, y))
						spots.Add(("" + plane[x + 1, y] + plane[x + 2, y], new Point(x, y)));
					else if (IsLetter(x, y + 1) && IsLetter(x, y + 2))
						spots.Add(("" + plane[x, y + 1] + plane[x, y + 2], new Point(x, y)));
				}
			}
		}

		var nameLookup = spots.ToLookup(s => s.name, s => s.position);

		foreach (var g in nameLookup) {
			if (g.Count() != 2) continue;
			Warps[g.First()] = g.Last();
			Warps[g.Last()] = g.First();
		}

		Target = nameLookup["ZZ"].Single();
		var start = new State { Position = nameLookup["AA"].Single() };
		Frontier.Enqueue(start);
	}

	protected override Point GetKey(State state) => state.Position;

	protected override bool IsGoal(State state) => state.Position.Equals(Target);

	protected override IEnumerable<State> NextStates(State state) {
		var pos = state.Position;
		int nm = state.Moves + 1;
		if (Warps.TryGetValue(pos, out var t))
			yield return new State { Position = t, Moves = nm };

		foreach (var next in pos.Neighbors)
			if (Plane[next] == '.') yield return new State { Position = next, Moves = nm };
	}
}