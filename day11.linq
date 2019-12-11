<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var board = new DefaultDictionary<(int, int), int>(_ => 0);
var painted = new HashSet<(int, int)>();
var machine = new IntCodeMachine();
int xmin = 0, xmax = 0;
int ymin = 0, ymax = 0;
bool painting = true;

int x = 0, y = 0, dir = 0;

machine.NextInput = () => board[(x, y)];

machine.Outputting += num => {
	if (painting) {
		painted.Add((x, y));
		board[(x, y)] = (int)num;
	}
	else {
		dir += (int)num switch { 0 => 3, 1 => 1, _ => throw new IndexOutOfRangeException() };
		switch (dir %= 4) {
			case 0: y -= 1; ymin = Min(ymin, y); break;
			case 1: x += 1; xmax = Max(xmax, x); break;
			case 2: y += 1; ymax = Max(ymax, y); break;
			case 3: x -= 1; xmin = Min(xmin, x); break;
		}
	}
	painting ^= true;
};

machine.Reset();
machine.Run();

WriteLine(painted.Count);

dir = x = y = xmin = xmax = ymin = ymax = 0;
painting = true;
board.Clear();
board[(0, 0)] = 1;
machine.Reset();
machine.Run();

for (int y_ = 0; y_ <= ymax - ymin; y_++) {
	for (int x_ = 0; x_ <= xmax - xmin; x_++) {
		Write(board[(x_,y_)] > 0 ? '#' : ' ');
	}
	WriteLine();
}
