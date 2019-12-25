<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
machine.Outputting += n => Write((char)n);

machine.TakeInput(@"
south
take spool of cat6
west
take space heater
south
take shell
north
north
take weather machine
north
west
west
take whirled peas
east
east
south
west
south
east
take candy cane
west
south
take space law space brochure
north
north
east
south
east
east
south
take hypercube
south
south
inv
".TrimStart().Replace("\r", ""));

string[] items = {"weather machine", "candy cane", "whirled peas", "hypercube", "space law space brochure", "space heater", "spool of cat6", "shell",};
for (int i = 0; i < 1 << items.Length; i++) {
	for (int j = 0; j < items.Length; j++) {
		if ((i >> j & 1) == 1)
			machine.TakeInputLine($"take {items[j]}");
		else
			machine.TakeInputLine($"drop {items[j]}");
	}
	machine.TakeInputLine("east");
}

while (!machine.Terminated) {
	while (machine.Tick()) ;
	machine.TakeInputLine(Console.ReadLine());
}
