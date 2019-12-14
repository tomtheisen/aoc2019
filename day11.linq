<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

int x = 0, y = 0, dir = 0;
int xmin = 0, xmax = 0;
int ymin = 0, ymax = 0;
var board = new Plane<int>(0, (0, ' '), (1, '#'));
var painted = new HashSet<(int, int)>();
var machine = new IntCodeMachine { NextInput = () => board[x, y] };

void Run() {
	for(machine.Reset();;) {
		painted.Add((x, y));
		if (!(machine.GetOutput() is BigInteger output)) return;
		board[x, y] = (int)output;
	
		dir += (int)machine.GetOutput()! switch { 0 => 3, 1 => 1, _ => throw new IndexOutOfRangeException() };
		switch (dir %= 4) {
			case 0: y -= 1; ymin = Min(ymin, y); break;
			case 1: x += 1; xmax = Max(xmax, x); break;
			case 2: y += 1; ymax = Max(ymax, y); break;
			case 3: x -= 1; xmin = Min(xmin, x); break;
		}
	}
}

Run();
WriteLine(painted.Count);

dir = x = y = xmin = xmax = ymin = ymax = 0;
board.Clear();
board[0, 0] = 1;
Run();

board.Dump();
