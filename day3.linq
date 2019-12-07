<Query Kind="Statements" />

#load ".\helpers.linq"

(int row, int col) Step(char dir, ref int row, ref int col) {
	switch (dir) {
		case 'U': row -= 1; break;
		case 'D': row += 1; break;
		case 'L': col -= 1; break;
		case 'R': col += 1; break;
	}
	return (row, col);
}

{
	var visited = new Dictionary<(int row, int col), int>();
	int row, col, best = int.MaxValue, wbit = 1;
	foreach (var line in GetAocLines()) {
		row = col = 0;
		foreach (var dir in line.Split(',')) {
			int dist = int.Parse(dir.Substring(1));
			for (int i = 0; i < dist; i++) {
				var pos = Step(dir[0], ref row, ref col);
				if (!visited.ContainsKey(pos)) visited.Add(pos, 0);
				visited[pos] |= wbit;
				if (visited[pos] > wbit) best = Math.Min(best, Math.Abs(row) + Math.Abs(col));
			}
		}
		wbit <<= 1;
	}
	Console.WriteLine(best);
}
{
	Dictionary<(int row, int col), int> GetWire(string line) {
		int row = 0, col = 0, wsteps = 1;
		var result = new Dictionary<(int row, int col), int>();
		foreach (var dir in line.Split(',')) {
			int dist = int.Parse(dir.Substring(1));
			for (int i = 0; i < dist; i++, wsteps++) 
				if (!result.ContainsKey(Step(dir[0], ref row, ref col))) 
					result.Add((row, col), wsteps);
		}
		return result;
	}
	
	int best = int.MaxValue;
	var wires = GetAocLines().Select(GetWire).ToArray();
	
	foreach (var (pos, steps) in wires[0]) {
		if (wires[1].ContainsKey(pos)) best = Math.Min(best, steps + wires[1][pos]);
	}
	Console.WriteLine(best);
}