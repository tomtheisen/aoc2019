<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

const bool animate = true;
const char Wall = '█', Floor = '░', Fog = '?', Drone = '•', O2 = '≈';

Direction[] AllDirections = new [] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

void Main() {
 	// part 1
	var steps = new Dictionary<Point, int> {[default] = 0};
	var machine = new IntCodeMachine();
	var plane = new Plane<char>(Fog) { [0, 0] = Drone };
	
	var planeContainer = animate ? new DumpContainer(plane).Dump() : null;
	Point pos = default, oxpos = default;
	
	Direction? GetMove() {
		var visited = new HashSet<Point> { pos };
		var frontier = new Queue<(Direction firstStep, Point position)>();

		foreach (var dir in AllDirections) frontier.Enqueue((dir, pos.Neighbor(dir)));
		while (frontier.Any()) {
			var (firstStep, pos) = frontier.Dequeue();
			if (plane[pos] == Wall || visited.Contains(pos)) continue;
			visited.Add(pos);
			if (plane[pos] == Fog) return firstStep;
			foreach (var dir in AllDirections)
				frontier.Enqueue((firstStep, pos.Neighbor(dir)));
		}
		return null;
	}
	
	while (true) {
		var dir = GetMove();
		if (dir == null) break;
		machine.TakeInput(dir switch {
			Direction.Up => 1,
			Direction.Down => 2,
			Direction.Left => 3,
			Direction.Right => 4,
		});
		Point lastPos = pos, target = pos.Neighbor(dir.Value);
		switch (machine.GetOutput() ?? throw new Exception("halted?")) {
			case 0: // failed to move
				plane[target] = Wall;
				break;
			case 1: // regular move
				if (plane[pos] == Drone) plane[pos] = Floor;
				pos = target;
				plane[pos] = Drone;
				if (!steps.ContainsKey(pos)) steps[pos] = steps[lastPos] + 1;
				break;
			case 2: // moved to o2
				plane[pos] = Floor;
				plane[target] = O2;
				oxpos = pos = target;
				if (!steps.ContainsKey(pos))
					WriteLine($"Oxygen@{pos} steps:{ steps[pos] = steps[lastPos] + 1 }");
				break;
		}
		
		planeContainer?.Refresh();
		if (animate) Thread.Sleep(10);
	}
	
	// part 2
	plane[pos] = plane[oxpos] = Floor;
	// discount one for the first, and one or the extra iteration to notice saturation
	int minutes = -2; 
	for (var frontier = new List<Point>{ oxpos }; frontier.Any(); minutes++) {
		var newFrontier = new List<Point>();
		foreach (var fpos in frontier) {
			if (plane[fpos] != Floor) continue;
			plane[fpos] = O2;
			foreach (var dir in AllDirections) newFrontier.Add(fpos.Neighbor(dir));
		}
		frontier = newFrontier;
		planeContainer?.Refresh();
		if (animate) Thread.Sleep(10);
	}
	WriteLine($"Total O2 in {minutes} minutes");
}
