<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

int x = 0, y = 0, dir = 0;
var board = new Plane<long>(0, (0, ' '), (1, '#'));
var machine = new IntCodeMachine { NextInput = () => board[x, y] };

void Run() {
	for(machine.Reset();;) {
		if (!(machine.GetOutput() is long output)) return;
		board[x, y] = output;

		dir += (int)machine.GetOutput()! switch { 0 => 3, 1 => 1 };
		switch (dir %= 4) {
			case 0: --y; break;
			case 1: ++x; break;
			case 2: ++y; break;
			case 3: --x; break;
		}
	}
}

Run();
WriteLine(board.Count);

dir = x = y = 0;
board.Clear();
board[0, 0] = 1;
Run();

board.Dump();