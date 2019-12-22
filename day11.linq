<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

Bearing bearing = default;
var board = new Plane<long>(0, (0, ' '), (1, '#'));
var machine = new IntCodeMachine { NextInput = () => board[bearing.Position] };

void Run() {
	for(machine.Reset();;) {
		if (!(machine.GetOutput() is long output)) return;
		board[bearing.Position] = output;
		bearing = machine.GetOutput() switch { 
			0 => bearing.TurnLeft.Forward, 
			1 => bearing.TurnRight.Forward, 
		};
	}
}

Run();
WriteLine(board.Count);

bearing = default;
board.Clear();
board[0, 0] = 1;
Run();

board.Dump();
