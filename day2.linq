<Query Kind="Statements" />

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine(GetAocIntegers());
Console.WriteLine(machine.Run(12, 2)[0]);

for (int noun = 0; noun < 100; noun++) {
	for (int verb = 0; verb < 100; verb++) {
		machine.Reset();
		var mem = machine.Run(noun, verb);
		if (mem[0] == 19690720) Console.WriteLine(100 * noun + verb);
	}
}