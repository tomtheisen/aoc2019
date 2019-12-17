<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
machine.Poke(0, 2);
machine.Outputting += n => {
	if (n < 127) Write((char)n);
	else WriteLine(n);
};

machine.TakeInput(@"
C,B,C,B,A,A,B,C,B,A
R,12,L,12,R,6
R,10,R,12,L,12
L,10,R,10,L,10,L,10
".TrimStart().Replace("\r", ""));
machine.TakeInput("n\n"); //video

machine.Run();
