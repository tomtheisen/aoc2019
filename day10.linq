<Query Kind="Statements">
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

var field = GetAocLines();
int width = field[0].Length, height = field.Length, best = -1, bestrow = -1, bestcol = -1;
var locations = new List<(int row, int col)>();
var angles = new HashSet<double>();

for (int i = 0; i < height; i++) for (int j = 0; j < width; j++) {
	if (field[i][j] != '#') continue;
	locations.Add((i, j));
	
	angles.Clear();
	for (int k = 0; k < height; k++) for (int l = 0; l < width; l++)
		if (field[k][l] == '#') angles.Add(Math.Atan2(k - i, l - j));
	
	if (angles.Count > best) (best, bestrow, bestcol) = (angles.Count, i, j);
}

$"{(bestcol, bestrow)}: {best}".Dump();

var ordered = new Queue<(int row, int col)[]>(
	locations
	.GroupBy(pt => {
		if (pt.col == bestcol && pt.row < bestrow) return -PI;
		return Atan2(bestcol - pt.col, pt.row - bestrow);
	})
	.OrderBy(kvp => kvp.Key)
	.Select(kvp => kvp.OrderBy(pt => Abs(pt.row - bestrow) + Abs(pt.col - bestcol)).ToArray())
);
	
for (int idx = 1; ordered.Any(); idx++) {
	var next = ordered.Dequeue();
	if (idx == 200) {
		var (row, col) = next.First();
		Console.WriteLine("{0}: {1}", idx, col * 100 + row);
	}
	if (next.Any()) ordered.Enqueue(next[1..]);
}
