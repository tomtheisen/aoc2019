<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
machine.Outputting += o => {
	if (o < 127) Write((char)o);
	else WriteLine(o);
};

// (!a | !b | !c) & d
machine.TakeInput(@"
NOT C J
NOT A T
OR T J
NOT B T
OR T J
AND D J
WALK
".TrimStart().Replace("\r", ""));
machine.Run();

// (!a | !b | (!c & h)) & d
machine.Reset();
machine.TakeInput(@"
NOT C J
AND H J
NOT A T
OR T J
NOT B T
OR T J
AND D J
RUN
".TrimStart().Replace("\r", ""));
machine.Run();
