<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var board = new Plane<int>(0, (0, ' '), (1, '▓'), (2, '◘'), (3, '═'), (4, '○'));

int maxBlocks = 0, blocks = 0, ballx = 0, padx = 0;

var score = new DumpContainer(0).Dump("Score");
var blocksContainer = new DumpContainer().Dump("Blocks");
var boardContainer = new DumpContainer(board).Dump("Board");
void ShowBoard() {
	boardContainer.Refresh();
    Thread.Sleep(30);
}

var machine = new IntCodeMachine { NextInput = () => Sign(ballx - padx)};
machine.Poke(0, 2);
while (true) {
	var x = machine.GetOutput();
	var y = machine.GetOutput();
	var tile = machine.GetOutput();
	if (tile == null) break;

	var pos = ((int)x, (int)y);
	if (pos == (-1, 0)) {
		score.Content = tile;
		continue;
	}
	switch (tile) {
		case 0:
			if (board[(int)x, (int)y] == 2) blocksContainer.Content = $"{--blocks} / {maxBlocks}";
			break;
		case 2:
			if (board[(int)x, (int)y] != 2) blocksContainer.Content = $"{++blocks} / {maxBlocks = Max(blocks, maxBlocks)}";
			break;
		case 3:
			padx = (int)x;
			board[(int)x, (int)y] = 3;
			ShowBoard();
			break;
		case 4:
			ballx = (int)x;
			break;
	}
	board[(int)x, (int)y] = (int)tile;
}
ShowBoard();